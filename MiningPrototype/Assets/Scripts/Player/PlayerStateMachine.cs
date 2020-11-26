using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

[System.Serializable]
public class PlayerVisualState
{
    public string StateName;
    public SpriteAnimation BodyAnimation;
    public HeadState HeadState;
    public HandsState HandsState;
    public CarryItemState CarryItemState;
    public AnimationPickaxeState PickaxeState;
}

public enum HeadState
{
    Integrated, Dynamic
}

public enum HandsState
{
    Integrated, Conditional
}

public enum CarryItemState
{
    Hidden, Conditional, ConditionalBehindPlayer
}

public enum AnimationPickaxeState
{
    Integrated, Behind, Conditional
}

[System.Serializable]
public struct PlayerStateInfo
{
    public string StateName;
    public bool CanInteract;
    public bool CanInventory;
}

[DefaultExecutionOrder(-20)]
public class PlayerStateMachine : StateListenerBehaviour, IStateMachineUser, IEntity
{
    [SerializeField] PlayerSettings settings;
    [SerializeField] Transform feet;
    [SerializeField] AudioSource walking, jumpStart, jumpLand;
    [SerializeField] bool slowWalkMode;
    [SerializeField] PlayerStateInfo[] statesCanInteract;

    [SerializeField] PlayerInteractionHandler playerInteraction;
    [SerializeField] bool debug;

    [Inject] ProgressionHandler progressionHandler;
    [Inject] TransitionEffectHandler transitionEffectHandler;

    StateMachine stateMachine;
    StateMachine.State s_idle, s_jump, s_fall, s_walk, s_slowWalk, s_climb, s_climbIde, s_inventory, s_death, s_hit, s_longIdle, s_disabled;
    Dictionary<string, PlayerStateInfo> canInteractInStateMap;
    public event System.Action PlayerDeath;

    private List<Ladder> currentLadders = new List<Ladder>();
    private float gravityScale;
    float lastGroundedTimeStamp;
    float lastJumpTimeStamp;
    float lastActivityTimeStamp;
    float lastDeathTimeStamp;
    float lastMineTimeStamp;

    private bool isGrounded;
    Vector2 rightWalkVector = Vector3.right;
    Rigidbody2D rigidbody;
    float horizontalSpeed;
    RuntimeProceduralMap.MirrorState currentMirrorLoc;
    private bool InFrontOfLadder { get => currentLadders.Count > 0; }
    private bool IsLocked { get => stateMachine.CurrentState == s_disabled; }


    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        gravityScale = rigidbody.gravityScale;

        SetupStateMachine();
        stateMachine.Start();

        canInteractInStateMap = new Dictionary<string, PlayerStateInfo>();

        foreach (var val in statesCanInteract)
        {
            canInteractInStateMap.Add(val.StateName, val);
        }

        playerInteraction.PlayerActivity += NotifyActivity;
    }

    protected override void OnNewGame()
    {
        var start = LocationIndicator.Find(IndicatorType.PlayerStart);
        if (start != null)
        {
            transform.position = start.transform.position;
        }
    }

    public void NotifyActivity()
    {
        lastActivityTimeStamp = Time.time;
    }

    private void OnGUI()
    {
        if (!debug)
            return;

        GUI.color = Color.black;
        GUI.Label(new Rect(10, 10, 100, 25), stateMachine.CurrentState.Name);
        GUI.Label(new Rect(110, 10, 100, 25), isGrounded ? "Grounded" : "Not Grounded");
        GUI.Label(new Rect(210, 10, 100, 25), currentMirrorLoc.ToString());

        float y = 40;
        for (int i = 0; i < stateMachine.States.Count; i++)
        {
            var s = stateMachine.States[i];

            if (GUI.Button(new Rect(10, y, 200, 25), s.Name))
            {
                stateMachine.ForceTransitionTo(s);
            }

            y += 30;
        }
    }

    private void FixedUpdate()
    {
        if (!gameState.Playing)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(feet.position, settings.feetRadius, settings.collisionMask.value);
        isGrounded = false;
        foreach (var c in colliders)
        {
            if (!c.isTrigger && c.gameObject != gameObject)
            {
                isGrounded = true;
                break;
            }
        }

        if (isGrounded)
        {
            lastGroundedTimeStamp = Time.time;
        }

        stateMachine.Update();
    }

    private void SetupStateMachine()
    {
        stateMachine = new StateMachine("PlayerStateMachine");

        s_idle = stateMachine.AddState("Idle", IdleEnter, MoveUpdate);
        s_jump = stateMachine.AddState("Jump", JumpEnter, MoveUpdate);
        s_walk = stateMachine.AddState("Walk", null, MoveUpdate, WalkExit);
        s_slowWalk = stateMachine.AddState("SlowWalk", null, SlowMoveUpdate, WalkExit);
        s_climb = stateMachine.AddState("Climb", ClimbingEnter, ClimbingUpdate, ClimbingExit);
        s_climbIde = stateMachine.AddState("ClimbIdle", ClimbingEnter, ClimbingUpdate, ClimbingExit);
        s_inventory = stateMachine.AddState("Inventory", null, MoveUpdate);
        s_death = stateMachine.AddState("Death", DeathEnter, DeathUpdate, DeathExit);
        s_hit = stateMachine.AddState("Hit", null);
        s_longIdle = stateMachine.AddState("LongIdle", null, SlowMoveUpdate);
        s_disabled = stateMachine.AddState("Disabled", null, null, DisableExit);
        s_fall = stateMachine.AddState("Fall", null, MoveUpdate, FallExit);

        s_idle.AddTransition(InInventory, s_inventory);
        s_inventory.AddTransition(() => !InInventory(), s_idle);

        s_idle.AddTransition(IsProlongedIdle, s_longIdle);
        s_longIdle.AddTransition(IsMoving, s_walk);
        s_longIdle.AddTransition(NotInProlongedIdle, s_idle);

        s_idle.AddTransition(IsFalling, s_fall);
        s_fall.AddTransition(IsGrounded, s_idle);
        s_jump.AddTransition(IsGrounded, s_idle);

        s_idle.AddTransition(ShouldJump, s_jump);
        s_walk.AddTransition(ShouldJump, s_jump);
        s_inventory.AddTransition(ShouldJump, s_jump);

        s_idle.AddTransition(IsMoving, s_walk);
        s_inventory.AddTransition(IsMoving, s_walk);
        s_walk.AddTransition(IsIdle, s_idle);
        s_walk.AddTransition(IsSlowWalking, s_slowWalk);
        s_slowWalk.AddTransition(IsIdle, s_idle);

        s_idle.AddTransition(ShouldClimb, s_climb);
        s_walk.AddTransition(ShouldClimb, s_climb);
        s_jump.AddTransition(ShouldClimb, s_climb);
        s_fall.AddTransition(ShouldClimb, s_climb);
        s_climb.AddTransition(IsNotClimbing, s_idle);
        s_climbIde.AddTransition(IsNotClimbing, s_idle);

        s_climb.AddTransition(IsClimbingIdle, s_climbIde);
        s_climbIde.AddTransition(IsMovingIdle, s_climb);

        s_jump.AddTransition(IsFalling, s_fall);

        s_hit.AddTransition(HitFinished, s_idle);
    }

    private void DisableExit()
    {
        NotifyActivity();
    }

    private void FallExit()
    {
        jumpLand?.Play();
    }

    private void WalkExit()
    {
        SetMovingSound(false);
    }

    private void DeathEnter()
    {
        transitionEffectHandler.FadeOut(FadeType.Death);
        rigidbody.simulated = false;
        lastDeathTimeStamp = Time.time;
        NotifyActivity();
        gameState.ChangeStateTo(GameState.State.Respawning);
        PlayerDeath?.Invoke();
    }

    private void DeathUpdate()
    {
        if (Time.time - lastDeathTimeStamp > settings.respawnCooldown)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        var bed = GameObject.FindObjectOfType<Bed>();

        if (bed != null)
        {
            transform.position = bed.transform.position;
            FindObjectOfType<CameraPanner>().UpdatePosition();
            stateMachine.ForceTransitionTo(s_idle);
            bed.BeginInteracting(gameObject);
            bed.WakeUpFromNightmare(gameObject);
            Debug.Log("Respawning at bed: " + bed.name);
        }
        else
        {
            Debug.LogError("No bed found to respawn");
            var pStart = GameObject.FindObjectOfType<LocationIndicator>();
            if (pStart != null)
            {
                transform.position = pStart.transform.position;
                stateMachine.ForceTransitionTo(s_longIdle);
            }
        }
    }

    private void DeathExit()
    {
        rigidbody.simulated = true;
        NotifyActivity();
        transitionEffectHandler.FadeIn(FadeType.Nightmare);
        gameState.ChangeStateTo(GameState.State.Playing);
    }

    private bool HitFinished()
    {
        return Time.time - lastActivityTimeStamp > settings.hitDuration;
    }

    private bool IsSlowWalking()
    {
        return slowWalkMode;
    }

    private void IdleEnter()
    {
        SetMovingSound(false);
    }

    private void JumpEnter()
    {
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, settings.jumpVelocity* progressionHandler.JumpMultiplyer);
        lastJumpTimeStamp = Time.time;
        jumpStart?.Play();
    }

    private void SlowMoveUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");

        rigidbody.position += horizontal * rightWalkVector * settings.slowMoveSpeed * Time.fixedDeltaTime * progressionHandler.SpeedMultiplyer;
        BaseMoveUpdate(horizontal);
    }

    private void MoveUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");

        rigidbody.position += horizontal * rightWalkVector * settings.moveSpeed * Time.fixedDeltaTime * progressionHandler.SpeedMultiplyer;
        BaseMoveUpdate(horizontal);
    }

    private void BaseMoveUpdate(float horizontal)
    {
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);

        horizontalSpeed = horizontal * rightWalkVector.x;

        if (IsMoving())
        {
            transform.localScale = new Vector3(horizontalSpeed > 0 ? 1 : -1, 1, 1);
            lastActivityTimeStamp = Time.time;
        }

        SetMovingSound(IsMoving() && IsGrounded());

        UpdateWorldMirroring();

        if (progressionHandler.IsMidas)
        {
            MidasUpdate();
        }

    }

    private void MidasUpdate()
    {
        var pos = transform.position.ToGridPosition() + new Vector2Int(0, -1);

        Util.DebugDrawTile(pos);
        var t = RuntimeProceduralMap.Instance[pos];

        if(t.Type == TileType.Stone)
        {
            RuntimeProceduralMap.Instance.SetMapAt(pos.x, pos.y, Tile.Make(TileType.Gold), TileUpdateReason.Place, updateProperties: true, updateVisuals: true);
        }
    }

    public void SetFaceDirection(bool right)
    {
        transform.localScale = new Vector3(right ? 1 : -1, 1, 1);
    }

    private void UpdateWorldMirroring()
    {
        if (rigidbody.position.x < 0)
        {
            rigidbody.position = new Vector2(rigidbody.position.x + RuntimeProceduralMap.Instance.SizeX, rigidbody.position.y);
        }
        else if (rigidbody.position.x > RuntimeProceduralMap.Instance.SizeX)
        {
            rigidbody.position = new Vector2(rigidbody.position.x - RuntimeProceduralMap.Instance.SizeX, rigidbody.position.y);
        }

        var oldMirrorLoc = currentMirrorLoc;
        currentMirrorLoc = GetMirrorLocation();
        if (currentMirrorLoc != oldMirrorLoc)
        {
            RuntimeProceduralMap.Instance.NotifyMirrorWorldSideChange(currentMirrorLoc);
        }
    }

    private RuntimeProceduralMap.MirrorState GetMirrorLocation()
    {
        if (rigidbody.position.x < RuntimeProceduralMap.Instance.SizeX / 3)
        {
            return RuntimeProceduralMap.MirrorState.Left;
        }
        else if (rigidbody.position.x > RuntimeProceduralMap.Instance.SizeX * 2 / 3)
        {
            return RuntimeProceduralMap.MirrorState.Right;
        }
        else
        {
            return RuntimeProceduralMap.MirrorState.Center;
        }
    }

    private void ClimbingEnter()
    {
        rigidbody.gravityScale = 0;
        NotifyActivity();

        if (currentLadders.Count > 0)
            currentLadders.ForEach((x) => x.NotifyUse());
    }

    private void ClimbingExit()
    {
        rigidbody.gravityScale = gravityScale;
        NotifyActivity();

        if (currentLadders.Count > 0)
            currentLadders.ForEach((x) => x.NotifyLeave());
    }

    private void ClimbingUpdate()
    {
        if (currentLadders == null)
            return;

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        Vector2 climbVelocity = new Vector2(horizontal * settings.climbPanSpeed, vertical * settings.climbSpeed);
        rigidbody.velocity = climbVelocity;
    }

    private bool IsGrounded()
    {
        return isGrounded && Time.time - lastJumpTimeStamp > 0.1f;
    }

    private bool ShouldClimb()
    {
        var vertical = Input.GetAxis("Vertical");
        bool up = vertical > 0;

        bool rightDirection = !(up ^ IsBelowTopLadder());

        return InFrontOfLadder && Mathf.Abs(vertical) > 0.75f && rightDirection;
    }

    private bool IsNotClimbing()
    {
        bool ca = !InFrontOfLadder;
        bool cb = (IsGrounded() && IsBelowTopLadder());

        return ca || cb ;
    }

    private bool IsBelowTopLadder()
    {
        var topL = GetTopLadder();
        if (topL == null)
            return false;

        return transform.position.y < (topL.transform.position.y + 5.5f); //hardcoded ladder height
    }

    private Ladder GetTopLadder()
    {
        if (currentLadders.Count == 0)
            return null;

        return currentLadders.OrderBy((x) => x.transform.position.y).First();
    }

    private bool IsIdle()
    {
        return !IsMoving();
    }

    private bool IsMoving()
    {
        return horizontalSpeed.Abs() > settings.idleThreshold;
    }

    private bool IsClimbingIdle()
    {
        return rigidbody.velocity.magnitude <= settings.climbIdleThreshold;
    }

    private bool IsMovingIdle()
    {
        return !IsClimbingIdle();
    }

    private bool ShouldJump()
    {
        var vertical = Input.GetAxis("Vertical");

        if (CanJump() && vertical > 0)
        {
            return true;
        }
        return false;
    }
    private bool CanJump()
    {
        return Time.time - lastGroundedTimeStamp < settings.timeAfterGroundedToJump && Time.time - lastJumpTimeStamp > settings.jumpCooldown;
    }

    private bool IsFalling()
    {
        return !isGrounded && rigidbody.velocity.y < 0;
    }

    private bool NotInProlongedIdle()
    {
        return !IsProlongedIdle();
    }

    private bool IsProlongedIdle()
    {
        return Time.time - lastActivityTimeStamp > settings.timeToLongIdle;
    }

    private bool InInventory()
    {
        return playerInteraction.InventoryDisplayState == InventoryState.Open;
    }

    public bool ShouldHoldPickaxe()
    {
        return Time.time - lastMineTimeStamp < (InOverworld() ? settings.overworldTimeToHidePickaxe : settings.timeToHidePickaxe);
    }

    public bool InOverworld()
    {
        return transform.position.y >= settings.overWorldHeight;
    }

    private bool InClimbState()
    {
        return stateMachine.CurrentState == s_climb || stateMachine.CurrentState == s_climbIde;
    }

    public void NotifyPickaxeUse()
    {
        lastMineTimeStamp = Time.time;
    }

    public StateMachine GetStateMachine()
    {
        return stateMachine;
    }

    private void SetMovingSound(bool on)
    {
        if (on)
        {
            if (!walking.isPlaying)
            {
                walking.Play();
            }
        }
        else
        {
            if (walking.isPlaying)
            {
                walking.Pause();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        rightWalkVector = Vector2.right;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Ladder ladder))
        {
            currentLadders.Add(ladder);
            if (InClimbState())
                ladder.NotifyUse();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Ladder ladder))
        {
            currentLadders.Remove(ladder);
            ladder.NotifyLeave();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateWalkVector(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateWalkVector(collision);
    }

    private void UpdateWalkVector(Collision2D collision)
    {
        var contact = collision.contacts[0];
        float angle = Mathf.Acos(Vector3.Dot(contact.normal, Vector3.up)) * Mathf.Rad2Deg;

        Debug.DrawLine(transform.position, transform.position + (Vector3)contact.normal);

        if (angle < settings.groundedAngle)
        {
            rightWalkVector = Vector3.Cross(contact.normal, Vector3.forward).normalized;
        }
        else
        {
            rightWalkVector = Vector3.right;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (feet != null)
            Gizmos.DrawWireSphere(feet.position, settings.feetRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightWalkVector);
    }

    public void TakeDamage(DamageStrength strength)
    {

        switch (strength)
        {
            case DamageStrength.Weak:
                stateMachine.ForceTransitionTo(s_hit);
                break;

            case DamageStrength.Strong:

                if (!isGrounded)
                    return;

                stateMachine.ForceTransitionTo(s_death);
                break;
        }
    }

    public bool CanInteract()
    {
        if (canInteractInStateMap.ContainsKey(stateMachine.CurrentState.Name))
            return canInteractInStateMap[stateMachine.CurrentState.Name].CanInteract;
        else
            Debug.LogError("No canInteract set for " + stateMachine.CurrentState.Name);


        return false;
    }

    public bool CanUseInventory()
    {
        if (canInteractInStateMap.ContainsKey(stateMachine.CurrentState.Name))
            return canInteractInStateMap[stateMachine.CurrentState.Name].CanInventory;
        else
            Debug.LogError("No canInventory set for " + stateMachine.CurrentState.Name);


        return false;
    }

    public void Disable()
    {
        stateMachine.ForceTransitionTo(s_disabled);
    }

    public void Enable()
    {
        stateMachine.ForceTransitionTo(s_idle);
    }

    public void ForceToState(string name)
    {
        stateMachine.ForceTransitionTo(name);
    }
}