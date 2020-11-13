using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngineInternal;

public enum PlayerState
{
    Normal,
    Locked,
    Climbing
}

public class PlayerController : InventoryOwner
{
    [Header("Player")]
    [SerializeField] float groundedAngle;
    [SerializeField] float jumpVelocity;
    [SerializeField] float moveSpeed;

    [SerializeField] float jumpCooldown = 0.1f;
    [SerializeField] float timeAfterGroundedToJump = 0.1f;

    [SerializeField] Transform feet;
    [SerializeField] float feetRadius;

    [SerializeField] TileMap generation;
    [SerializeField] float maxDigDistance = 3;

    [SerializeField] GameObject pickaxe;
    [SerializeField] float digSpeed = 10;
    [SerializeField] Transform mouseHighlight;

    [SerializeField] SpriteAnimation an_Walk, an_Idle, an_Fall, an_Inventory, an_Climb, an_ClimbIdle;

    [SerializeField] ParticleSystem miningParticles;
    [SerializeField] int miningBreakParticlesCount;
    [SerializeField] float miningParticlesRateOverTime = 4;

    [SerializeField] AudioSource breakBlock, startMining, walking;
    [SerializeField] DirectionBasedAnimator pickaxeAnimator;

    [SerializeField] float inventoryOpenDistance;
    [SerializeField] float maxInteractableDistance;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] float climbSpeed;
    [SerializeField] float climbPanSpeed;
    [SerializeField] float climbIdleThreshold;

    Rigidbody2D rigidbody;
    SpriteAnimator spriteAnimator;
    float lastGroundedTimeStamp;
    float lastJumpTimeStamp;

    private bool isGrounded;
    Vector2 rightWalkVector = Vector3.right;
    Camera camera;
    SpriteRenderer spriteRenderer;
    Vector2Int? gridDigTarget;
    IInteractable currentInteractable;
    IMinableNonGrid nonGridDigTarget;

    [ReadOnly]
    [SerializeField] bool inMining;

    private PlayerState state;
    private bool isVisible = true;
    private Ladder currentLadder;
    private float gravityScale;


    private bool InFrontOfLadder { get => currentLadder != null; }
    private bool IsLocked { get => state != PlayerState.Locked; }

    protected override void Start()
    {
        base.Start();
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteAnimator = GetComponent<SpriteAnimator>();
        gravityScale = rigidbody.gravityScale;
    }

    private void Update()
    {
        if (!IsLocked || !isVisible)
            return;

        if (Vector2Int.Distance(GetPositionInGrid(), GetClickCoordinate()) <= maxDigDistance)
        {
            Debug.DrawLine(GetPositionInGridV3(), GetClickPositionV3(), Color.yellow, Time.deltaTime);
            UpdateDigTarget();
            UpdateNonGridDigTarget();

            if (Input.GetMouseButton(0))
            {
                if (eventSystem.currentSelectedGameObject == null)
                    TryDig();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (Vector3.Distance(GetPositionInGridV3(), GetClickPositionV3()) <= inventoryOpenDistance && isGrounded)
                {
                    OpenInventory();
                }
                else
                {
                    if (currentInteractable == null)
                        TryInteract();
                    else
                        TryStopInteracting();
                }
            }
            else
            {
                TryDisableMiningVisuals();
            }
        }
        else
        {
            gridDigTarget = null;
            TryDisableMiningVisuals();
        }

        UpdateDigHighlight();
    }

    private void TryInteract()
    {
        var hits = DoRaycast2DFromMouse();

        currentInteractable = null;
        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                Debug.Log(hit.transform.name);
                currentInteractable = interactable;
                currentInteractable.SubscribeToForceQuit(OnInteractableForceQuit);
                currentInteractable.BeginInteracting(gameObject);
                Debug.DrawLine(GetPositionInGridV3(), hit.point, Color.green, 1f);
                break;
            }
        }
    }

    private RaycastHit2D[] DoRaycast2DFromMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray r = camera.ScreenPointToRay(mousePos);
        return Physics2D.RaycastAll(r.origin, r.direction, 100000);
    }

    private void OnInteractableForceQuit()
    {
        TryStopInteracting();
    }

    private bool CanJump()
    {
        return Time.time - lastGroundedTimeStamp < timeAfterGroundedToJump && Time.time - lastJumpTimeStamp > jumpCooldown;
    }

    private void UpdateDigTarget()
    {
        gridDigTarget = TileMapHelper.GetClosestSolidBlock(generation, GetPositionInGrid(), GetClickCoordinate());
        if (generation.IsAirAt(gridDigTarget.Value.x, gridDigTarget.Value.y))
        {
            gridDigTarget = null;
        }
    }

    private void UpdateNonGridDigTarget()
    {
        var hits = DoRaycast2DFromMouse();

        IMinableNonGrid newTarget = null;
        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.TryGetComponent(out IMinableNonGrid minable))
            {
                Debug.Log(hit.transform.name);
                newTarget = minable;
                break;
            }
        }

        //Debug.Log("new target: " + (newTarget != null).ToString() + " / old target:" + (nonGridDigTarget != null).ToString());

        if (newTarget != nonGridDigTarget)
        {
            if (nonGridDigTarget != null)
                nonGridDigTarget.MouseLeave();

            if (newTarget != null)
                newTarget.MouseEnter();

            nonGridDigTarget = newTarget;
        }
    }

    private void UpdateDigHighlight()
    {

        if (gridDigTarget == null)
        {
            if (nonGridDigTarget == null)
                mouseHighlight.position = new Vector3(-1000, -1000);
            else
                mouseHighlight.position = new Vector3(nonGridDigTarget.GetPosition().x, nonGridDigTarget.GetPosition().y, 0);
        }
        else
            mouseHighlight.position = new Vector3(gridDigTarget.Value.x, gridDigTarget.Value.y, 0) + new Vector3(0.5f, 0.5f, 0);
    }

    private void TryPlace()
    {
        Vector2Int clickPos = GetClickCoordinate();
        if (TileMapHelper.HasLineOfSight(generation, GetPositionInGrid(), clickPos, debugVisualize: true))
            generation.PlaceAt(clickPos.x, clickPos.y);
    }

    private void TryDig()
    {
        CloseInventory();

        if (gridDigTarget.HasValue)
        {
            bool broken = generation.DamageAt(gridDigTarget.Value.x, gridDigTarget.Value.y, Time.deltaTime * digSpeed);

            if (broken)
            {
                miningParticles.transform.position = (Vector3Int)gridDigTarget + new Vector3(0.5f, 0.5f);
                miningParticles.Emit(miningBreakParticlesCount);
                breakBlock.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                breakBlock.Play();
                TryDisableMiningVisuals();
            }
            else
            {
                UpdateMiningParticlesPositions();
            }

            TryEnableMiningVisuals();
        }
        else
        {
            if (nonGridDigTarget != null)
            {
                nonGridDigTarget.Damage(Time.deltaTime * digSpeed);

                TryEnableMiningVisuals();
            }
            else
            {
                TryDisableMiningVisuals();
            }
        }
    }

    private void UpdateMiningParticlesPositions()
    {
        miningParticles.transform.position = TileMapHelper.GetWorldLocationOfFreeFaceFromSource(generation, gridDigTarget.Value, GetPositionInGrid());
        Debug.DrawLine((Vector3Int)GetPositionInGrid(), miningParticles.transform.position, Color.yellow, 0.1f);
    }

    private void TryDisableMiningVisuals()
    {
        if (inMining)
        {
            inMining = false;
            var emission = miningParticles.emission;
            emission.rateOverTimeMultiplier = 0;
            startMining.Stop();
            pickaxeAnimator.Stop();
        }
    }

    private void TryStopInteracting()
    {
        if (currentInteractable != null)
        {
            currentInteractable.EndInteracting(gameObject);
            currentInteractable.UnsubscribeToForceQuit(OnInteractableForceQuit);
            currentInteractable = null;
        }
    }

    private void TryEnableMiningVisuals()
    {
        if (!inMining)
        {
            var emission = miningParticles.emission;
            emission.rateOverTimeMultiplier = miningParticlesRateOverTime;
            inMining = true;
            startMining.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            startMining.Play();
            pickaxeAnimator.Play();
        }
    }

    public Vector2Int GetPositionInGrid()
    {
        return transform.position.ToGridPosition() + new Vector2Int(0, 1); //+1 to be at center of player
    }

    /// <summary>
    /// Just +1 in Y compared to transform.position
    /// </summary>
    public Vector3 GetPositionInGridV3()
    {
        return new Vector3(transform.position.x, transform.position.y + 1); //+1 to be at center of player
    }

    private Vector2Int GetClickCoordinate()
    {
        Vector3 clickPos = GetClickPositionV3();
        return new Vector2Int((int)clickPos.x, (int)clickPos.y);
    }

    private Vector3 GetClickPositionV3()
    {
        Vector3 position = Input.mousePosition + Vector3.back * camera.transform.position.z;
        return camera.ScreenToWorldPoint(position);
    }

    private void FixedUpdate()
    {
        if (!isVisible)
            return;

        switch (state)
        {
            case PlayerState.Normal:
                UpdateWalk();
                if (InFrontOfLadder)
                    TryStartClimb();
                else
                    UpdateJump();
                break;

            case PlayerState.Climbing:
                UpdateClimb();
                break;
        }
    }

    private void UpdateClimb()
    {
        if (InFrontOfLadder)
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            Vector2 climbVelocity = new Vector2(horizontal * climbPanSpeed, vertical * climbSpeed);
            rigidbody.velocity = climbVelocity;

            if (climbVelocity.magnitude > climbIdleThreshold)
            {
                spriteAnimator.Play(an_Climb, false);
            }
            else
            {
                spriteAnimator.Play(an_ClimbIdle, false);
            }

            if (vertical > 0)
                currentLadder.NotifyGoingUp();
            else
                currentLadder.NotifyGoingDown();

        }
        else
        {
            ChangeStateTo(PlayerState.Normal);
        }


    }

    private void TryStartClimb()
    {
        var vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(vertical) > 0.75f)
        {
            ChangeStateTo(PlayerState.Climbing);
        }
    }

    private void UpdateWalk()
    {
        var horizontal = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontal) > 0.15f)
        {
            CloseInventory();
        }

        if (currentInteractable != null)
        {
            if (Vector3.Distance(GetPositionInGridV3(), currentInteractable.gameObject.transform.position) > maxInteractableDistance)
                TryStopInteracting();
        }

        rigidbody.position += horizontal * rightWalkVector * moveSpeed * Time.fixedDeltaTime;
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);

        if (Mathf.Abs(horizontal) > 0.2f)
            spriteRenderer.flipX = horizontal < 0;

        if (isGrounded)
        {
            if (horizontal == 0)
            {
                if (InventoryDisplayState == InventoryState.Open)
                {
                    spriteAnimator.Play(an_Inventory, false);
                    SetPickaxeVisible(false);
                }
                else
                {
                    spriteAnimator.Play(an_Idle, false);
                    SetPickaxeVisible(true);
                }
            }
            else
            {
                spriteAnimator.Play(an_Walk, false);
                SetPickaxeVisible(true);
            }
        }
        else
        {
            spriteAnimator.Play(an_Fall);
            SetPickaxeVisible(true);
        }

        UpdateWalkingSound(horizontal);
    }

    private void SetPickaxeVisible(bool isVisible = true)
    {
        if (isVisible != pickaxe.activeSelf)
            pickaxe.SetActive(isVisible);
    }

    private void UpdateJump()
    {
        var vertical = Input.GetAxis("Vertical");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(feet.position, feetRadius);
        isGrounded = colliders != null && colliders.Length > 1;

        if (isGrounded)
        {
            lastGroundedTimeStamp = Time.time;
        }

        if (CanJump() && vertical > 0)
        {
            Jump();
        }
    }

    private void Jump()
    {
        Debug.Log("Jump");
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpVelocity);
        lastJumpTimeStamp = Time.time;
    }

    private void UpdateWalkingSound(float horizontal)
    {
        if (isGrounded && Mathf.Abs(horizontal) > 0.01f)
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

    private void ChangeStateTo(PlayerState newState)
    {
        if (state == newState)
            return;

        LeaveState(state);
        state = newState;
        EnterState(newState);
    }

    private void LeaveState(PlayerState stateLeft)
    {
        switch (stateLeft)
        {
            case PlayerState.Climbing:
                rigidbody.gravityScale = gravityScale;
                SetPickaxeVisible(true);
                break;
        }
    }

    private void EnterState(PlayerState stateEntered)
    {
        switch (stateEntered)
        {
            case PlayerState.Climbing:
                SetPickaxeVisible(false);
                rigidbody.gravityScale = 0; 
                break;
        }
    }

    [Button]
    public void Hide()
    {
        isVisible = false;
        SetPickaxeVisible(false);
        spriteRenderer.enabled = false;
        walking.Pause();
    }

    [Button]
    public void Show()
    {
        isVisible = true;
        SetPickaxeVisible(false);
        spriteRenderer.enabled = true;
    }

    [Button]
    public void Freeze()
    {
        ChangeStateTo(PlayerState.Locked);
        walking.Pause();
    }

    [Button]
    public void Defreeze()
    {
        ChangeStateTo(PlayerState.Normal);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateWalkVector(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateWalkVector(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        rightWalkVector = Vector2.right;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Ladder ladder))
        {
            currentLadder = ladder;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Ladder ladder))
        {
            currentLadder = null;
        }
    }

    private void UpdateWalkVector(Collision2D collision)
    {
        var contact = collision.contacts[0];
        float angle = Mathf.Acos(Vector3.Dot(contact.normal, Vector3.up)) * Mathf.Rad2Deg;

        Debug.DrawLine(transform.position, transform.position + (Vector3)contact.normal);

        if (angle < groundedAngle)
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
            Gizmos.DrawWireSphere(feet.position, feetRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightWalkVector);

        Gizmos.DrawWireSphere((Vector3Int)GetPositionInGrid(), maxDigDistance);
        Gizmos.DrawWireSphere(GetPositionInGridV3(), inventoryOpenDistance);
    }
}
