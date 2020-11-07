using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody;

    [SerializeField] float groundedAngle;
    [SerializeField] float jumpVelocity;
    [SerializeField] float moveSpeed;

    [SerializeField] float jumpCooldown = 0.1f;
    [SerializeField] float timeAfterGroundedToJump = 0.1f;

    [SerializeField] Transform feet;
    [SerializeField] float feetRadius;

    [SerializeField] TestGeneration generation;
    [SerializeField] float maxDigDistance = 3;

    [SerializeField] float digSpeed = 10;
    [SerializeField] Transform mouseHighlight;

    [SerializeField] SpriteAnimation an_Walk, an_Idle, an_Fall;

    [SerializeField] ParticleSystem miningParticles;
    [SerializeField] int miningBreakParticlesCount;
    [SerializeField] float miningParticlesRateOverTime = 4;

    [SerializeField] AudioSource breakBlock, startMining;

    SpriteAnimator spriteAnimator;
    float lastGroundedTimeStamp;
    float lastJumpTimeStamp;

    private bool isGrounded;
    Vector2 rightWalkVector = Vector3.right;
    Camera camera;
    SpriteRenderer spriteRenderer;
    Vector2Int? digTarget;

    [ReadOnly]
    [SerializeField] bool inMining;

    private void Start()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteAnimator = GetComponent<SpriteAnimator>();
    }



    private void Update()
    {

        if (Vector2Int.Distance(GetPositionInGrid(), GetClickPosition()) <= maxDigDistance)
        {
            UpdateDigTarget();

            if (Input.GetMouseButton(0))
            {
                TryDig();
            }
            else if (Input.GetMouseButton(1))
            {
                TryPlace();
            }
            else
            {
                if (inMining)
                    DisableMiningParticles();
            }
        }
        else
        {
            digTarget = null;
            if (inMining)
                DisableMiningParticles();
        }

        UpdateDigHighlight();
    }
    private bool CanJump()
    {
        return Time.time - lastGroundedTimeStamp < timeAfterGroundedToJump && Time.time - lastJumpTimeStamp > jumpCooldown;
    }

    private void UpdateDigTarget()
    {
        digTarget = generation.GetClosestSolidBlock(GetPositionInGrid(), GetClickPosition());
        if (generation.IsAirAt(digTarget.Value.x, digTarget.Value.y))
        {
            digTarget = null;
        }
    }

    private void UpdateDigHighlight()
    {

        if (digTarget == null)
            mouseHighlight.position = new Vector3(-1000, -1000);
        else
            mouseHighlight.position = new Vector3(digTarget.Value.x, digTarget.Value.y, 0) + new Vector3(0.5f, 0.5f, 0);
    }

    private void TryPlace()
    {
        Vector2Int clickPos = GetClickPosition();
        if (generation.HasLineOfSight(GetPositionInGrid(), clickPos, debugVisualize: true))
            generation.PlaceAt(clickPos.x, clickPos.y);
    }

    private void TryDig()
    {
        if (digTarget.HasValue)
        {
            bool broken = generation.DamageAt(digTarget.Value.x, digTarget.Value.y, Time.deltaTime * digSpeed);

            if (broken)
            {
                miningParticles.transform.position = (Vector3Int)digTarget + new Vector3(0.5f, 0.5f);
                miningParticles.Emit(miningBreakParticlesCount);
                breakBlock.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                breakBlock.Play();
                DisableMiningParticles();
            }
            else
            {
                UpdateMiningParticlesPositions();
            }

            if (!inMining)
            {
                StartMiningParticles();
            }
        }
        else
        {
            if(inMining)
            DisableMiningParticles();
        }
    }

    private void UpdateMiningParticlesPositions()
    {
        miningParticles.transform.position = generation.GetWorldLocationOfFreeFaceFromSource(digTarget.Value, GetPositionInGrid());
        Debug.DrawLine((Vector3Int)GetPositionInGrid(), miningParticles.transform.position, Color.yellow, 0.1f);
    }


    private void DisableMiningParticles()
    {
        inMining = false;
        var emission = miningParticles.emission;
        emission.rateOverTimeMultiplier = 0;
        startMining.Stop();
    }



    private void StartMiningParticles()
    {
        var emission = miningParticles.emission;
        emission.rateOverTimeMultiplier = miningParticlesRateOverTime;
        inMining = true;
        startMining.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        startMining.Play();
    }

    private Vector2Int GetPositionInGrid()
    {
        return new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) + 1); //+1 to be at center of player
    }

    private Vector2Int GetClickPosition()
    {
        Vector3 position = Input.mousePosition + Vector3.back * camera.transform.position.z;
        Vector3 clickPos = camera.ScreenToWorldPoint(position);
        return new Vector2Int((int)clickPos.x, (int)clickPos.y);
    }

    private void FixedUpdate()
    {
        UpdateWalk();
        UpdateJump();
    }

    private void UpdateWalk()
    {
        var horizontal = Input.GetAxis("Horizontal");

        rigidbody.position += horizontal * rightWalkVector * moveSpeed * Time.fixedDeltaTime;
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);

        if (Mathf.Abs(horizontal) > 0.2f)
            spriteRenderer.flipX = horizontal < 0;

        if (isGrounded)
        {
            if (horizontal == 0)
                spriteAnimator.Play(an_Idle, false);
            else
                spriteAnimator.Play(an_Walk, false);
        }
        else
        {
            spriteAnimator.Play(an_Fall);
        }
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

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightWalkVector);

        Gizmos.DrawWireSphere((Vector3Int)GetPositionInGrid(), maxDigDistance);
    }
}
