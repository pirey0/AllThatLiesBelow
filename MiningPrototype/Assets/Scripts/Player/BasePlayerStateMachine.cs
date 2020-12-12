using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BasePlayerStateMachine : StateListenerBehaviour, IStateMachineUser, IEntity
{
    [Header("Base Player State Machine")]

    [SerializeField] PlayerSettings settings;
    [SerializeField] Transform feet;
    [SerializeField] AudioSource walking, walkingSlow, jumpStart, jumpLand, fallDeath;

    [SerializeField] protected bool slowWalkMode;
    [SerializeField] bool debug;

    StateMachine stateMachine;
    StateMachine.State s_idle, s_crouchIdle, s_jump, s_fall, s_walk, s_slowWalk, s_crouchWalk, s_climb, s_climbIde, s_inventory, s_death, s_hit, s_longIdle, s_disabled, s_fallDeath;
    public event System.Action PlayerDeath;

    private List<Ladder> currentLadders = new List<Ladder>();
    private float gravityScale;
    float lastGroundedTimeStamp;
    float lastJumpTimeStamp;
    float lastActivityTimeStamp;
    float lastDeathTimeStamp;
    float lastMineTimeStamp;

    private bool isGrounded;
    private Vector2 oldVelocity;
    Vector2 rightWalkVector = Vector3.right;
    protected Rigidbody2D rigidbody;
    float horizontalSpeed;

    private bool InFrontOfLadder { get => currentLadders.Count > 0; }
    private bool IsLocked { get => stateMachine.CurrentState == s_disabled; }
    protected string CurrentStateName { get => stateMachine.CurrentState.Name; }

    protected virtual void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        gravityScale = rigidbody.gravityScale;

        SetupStateMachine();
        stateMachine.Start();
        Debug.Log("State Machine Setup");
    }

    protected abstract float GetVerticalInput();
    protected abstract float GetHorizontalInput();

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
        oldVelocity = rigidbody.velocity;
    }

    private void SetupStateMachine()
    {
        stateMachine = new StateMachine("PlayerStateMachine");

        s_idle = stateMachine.AddState("Idle", IdleEnter, MoveUpdate);
        s_crouchIdle = stateMachine.AddState("CrouchIdle", IdleEnter, MoveUpdate);
        s_jump = stateMachine.AddState("Jump", JumpEnter, MoveUpdate);
        s_walk = stateMachine.AddState("Walk", null, MoveUpdate, WalkExit);
        s_slowWalk = stateMachine.AddState("SlowWalk", null, SlowMoveUpdate, WalkExit);
        s_crouchWalk = stateMachine.AddState("CrouchWalk", null, SlowMoveUpdate, WalkExit);
        s_climb = stateMachine.AddState("Climb", ClimbingEnter, ClimbingUpdate, ClimbingExit);
        s_climbIde = stateMachine.AddState("ClimbIdle", ClimbingEnter, ClimbingUpdate, ClimbingExit);
        s_inventory = stateMachine.AddState("Inventory", null, MoveUpdate);
        s_death = stateMachine.AddState("Death", DeathEnter, DeathUpdate, DeathExit);
        s_fallDeath = stateMachine.AddState("FallDeath", DeathEnter, DeathUpdate, DeathExit);
        s_hit = stateMachine.AddState("Hit", null);
        s_longIdle = stateMachine.AddState("LongIdle", null, SlowMoveUpdate);
        s_disabled = stateMachine.AddState("Disabled", null, null, DisableExit);
        s_fall = stateMachine.AddState("Fall", null, MoveUpdate, FallExit);

        s_idle.AddTransition(InInventory, s_inventory);
        s_inventory.AddTransition(() => !InInventory(), s_idle);

        s_idle.AddTransition(IsProlongedIdle, s_longIdle);
        s_longIdle.AddTransition(IsMoving, s_walk);
        s_longIdle.AddTransition(NotInProlongedIdle, s_idle);

        s_walk.AddTransition(IsSlowWalking, s_slowWalk);
        s_slowWalk.AddTransition(IsIdle, s_idle);

        s_walk.AddTransition(ShouldCrouch, s_crouchWalk);
        s_crouchWalk.AddTransition(ShouldNotCrouch, s_walk);
        s_crouchWalk.AddTransition(IsIdle, s_crouchIdle);

        s_idle.AddTransition(ShouldCrouch, s_crouchIdle);
        s_crouchIdle.AddTransition(ShouldNotCrouch, s_idle);
        s_crouchIdle.AddTransition(IsMoving, s_crouchWalk);

        s_idle.AddTransition(IsFalling, s_fall);
        s_fall.AddTransition(IsGrounded, s_idle);
        s_jump.AddTransition(IsGrounded, s_idle);

        s_idle.AddTransition(ShouldJump, s_jump);
        s_walk.AddTransition(ShouldJump, s_jump);
        s_inventory.AddTransition(ShouldJump, s_jump);

        s_idle.AddTransition(IsMoving, s_walk);
        s_inventory.AddTransition(IsMoving, s_walk);
        s_walk.AddTransition(IsIdle, s_idle);


        s_idle.AddTransition(ShouldClimb, s_climb);
        s_walk.AddTransition(ShouldClimb, s_climb);
        s_jump.AddTransition(ShouldClimb, s_climb);
        s_fall.AddTransition(ShouldClimb, s_climb);
        s_climb.AddTransition(IsNotClimbing, s_idle);
        s_climbIde.AddTransition(IsNotClimbing, s_idle);

        s_climb.AddTransition(IsClimbingIdle, s_climbIde);
        s_climbIde.AddTransition(IsMovingIdle, s_climb);

        s_jump.AddTransition(IsFalling, s_fall);
        s_idle.AddTransition(IsFalling, s_fall);
        s_walk.AddTransition(IsFalling, s_fall);
        s_slowWalk.AddTransition(IsFalling, s_fall);
        s_crouchWalk.AddTransition(IsFalling, s_fall);

        s_hit.AddTransition(HitFinished, s_idle);
    }

    protected virtual bool ShouldCrouch()
    {
        return false;
    }

    private bool ShouldNotCrouch()
    {
        return !ShouldCrouch();
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

    protected virtual void DeathEnter()
    {
        fallDeath?.Play();
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
        gameState.ReloadScene();
    }

    protected virtual void DeathExit()
    {
        rigidbody.simulated = true;
        NotifyActivity();
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
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, settings.jumpVelocity * GetJumpMultiplyer());
        lastJumpTimeStamp = Time.time;
        jumpStart?.Play();
    }

    protected virtual float GetJumpMultiplyer()
    {
        return 1;
    }

    private void SlowMoveUpdate()
    {
        var horizontal = GetHorizontalInput();

        var movement = horizontal * rightWalkVector * settings.slowMoveSpeed * Time.fixedDeltaTime * GetSpeedMultiplyer();
        BaseMoveUpdate(horizontal, movement);
    }

    protected virtual float GetSpeedMultiplyer()
    {
        return 1;
    }

    private void MoveUpdate()
    {
        var horizontal = GetHorizontalInput();

        var movement = horizontal * rightWalkVector * settings.moveSpeed * Time.fixedDeltaTime * GetSpeedMultiplyer();
        BaseMoveUpdate(horizontal, movement);
    }

    protected virtual void BaseMoveUpdate(float horizontal, Vector2 movement)
    {
        Vector2 p1 = transform.position + new Vector3(0, 1f);
        Vector2 p2 = transform.position + new Vector3(0, 1.6f);
        Vector2 dir = horizontal > 0 ? rightWalkVector : -rightWalkVector;
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        hits.AddRange(Physics2D.RaycastAll(p1, dir, 0.7f, settings.collisionMask));
        hits.AddRange(Physics2D.RaycastAll(p2, dir, 0.7f, settings.collisionMask));
        Debug.DrawLine(p1, p1 + dir * 0.7f, Color.red, Time.deltaTime);
        Debug.DrawLine(p2, p2 + dir * 0.7f, Color.red, Time.deltaTime);

        foreach (var hit in hits)
        {
            if (!hit.collider.isTrigger && hit.transform.TryGetComponent<ITileMapElement>(out ITileMapElement el)) //allow walking into ball/crates etc
            {
                movement = Vector3.zero;
                horizontal = 0;
            }
        }

        rigidbody.position += movement;
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
        horizontalSpeed = horizontal * rightWalkVector.x;

        if (IsMoving())
        {
            transform.localScale = new Vector3(horizontalSpeed > 0 ? 1 : -1, 1, 1);
            lastActivityTimeStamp = Time.time;
        }

        SetMovingSound(IsMoving() && IsGrounded());

    }

    public void SetFaceDirection(bool right)
    {
        transform.localScale = new Vector3(right ? 1 : -1, 1, 1);
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

        var horizontal = GetHorizontalInput();
        var vertical = GetVerticalInput();

        Vector2 climbVelocity = new Vector2(horizontal * settings.climbPanSpeed, vertical * settings.climbSpeed);
        rigidbody.velocity = climbVelocity;
    }

    private bool IsGrounded()
    {
        return isGrounded && Time.time - lastJumpTimeStamp > 0.1f;
    }

    private bool ShouldClimb()
    {
        var vertical = GetVerticalInput();
        bool up = vertical > 0;

        bool rightDirection = !(up ^ IsBelowTopLadder());

        return InFrontOfLadder && Mathf.Abs(vertical) > 0.75f && rightDirection;
    }

    private bool IsNotClimbing()
    {
        bool ca = !InFrontOfLadder;
        bool cb = (IsGrounded() && IsBelowTopLadder());

        return ca || cb;
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
        var vertical = GetVerticalInput();

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

    protected virtual bool InInventory()
    {
        return false;
    }

    public bool ShouldHoldPickaxe()
    {
        return Time.time - lastMineTimeStamp < (InOverworld() ? settings.overworldTimeToHidePickaxe : settings.timeToHidePickaxe);
    }

    public bool InOverworld()
    {
        return transform.position.y >= Constants.OVERWORLD_START_Y;
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
            if (slowWalkMode)
            {
                TryPlay(walkingSlow);
                TryPause(walking);
            }
            else
            {
                TryPlay(walking);
                TryPause(walkingSlow);
            }
        }
        else
        {
            walkingSlow.Pause();
            walking.Pause();
        }
    }

    protected override void OnPaused()
    {
        SetMovingSound(false);
    }

    private void TryPlay(AudioSource aus)
    {
        if (!aus.isPlaying)
            aus.Play();
    }

    private void TryPause(AudioSource aus)
    {
        if (aus.isPlaying)
            aus.Pause();
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

        if (-oldVelocity.y > settings.fallSpeedThatKills)
        {
            Debug.Log("Killed from fall Damage with a speed of: " + -oldVelocity.y);
            stateMachine.ForceTransitionTo(s_fallDeath);
        }
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
                stateMachine.ForceTransitionTo(s_death);
                break;
        }
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