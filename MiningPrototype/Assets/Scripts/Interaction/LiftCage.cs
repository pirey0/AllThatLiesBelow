using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

public enum LiftState
{
    Active,
    Inactive
}

public class LiftCage : MonoBehaviour
{
    [BoxGroup("Positions")] [SerializeField] Vector3 leftRopeOffset, rightRopeOffset, leftRopeOffsetBase, rightRopeOffsetBase;

    [SerializeField] float liftMaxSpeed = 10;
    [BoxGroup("Animations")] [SerializeField] SpriteAnimator liftFG_anim, liftBG_anim;
    [BoxGroup("Animations")] [SerializeField] SpriteAnimator[] wheels_anim;
    [BoxGroup("Animations")] [SerializeField] SpriteAnimation liftFG_active, liftFG_inactive, liftBG_active, LiftBG_inactive, LiftWheel_active_left, LiftWheel_active_right, LiftWheel_inactive;
    [SerializeField] SpriteRenderer ropeRenderer;
    [SerializeField] Transform liftBase;
    [SerializeField] LineRenderer leftRope, rightRope;
    [SerializeField] DistanceJoint2D distanceJoint;

    [SerializeField] AudioSource movingUpAndDownSound;
    [SerializeField] ParticleSystem smokeSystem;

    [SerializeField] LiftState state = LiftState.Inactive;
    Direction direction;

    float liftSpeed = 0;
    Vector3 playerOffset;


    LiftState State
    {
        get => state;
        set
        {
            if (state == value)
                return;

            state = value;
            AdaptVisualsToState(state);
        }
    }

    private void AdaptVisualsToState(LiftState state)
    {
        liftFG_anim.Play((state == LiftState.Active) ? liftFG_active:  liftFG_inactive);
        liftBG_anim.Play((state == LiftState.Active) ? liftBG_active : LiftBG_inactive);
    }

    private void UpdateCables()
    {
        //float height = Mathf.Abs(liftOrigin.y - (transform.position + Vector3.up).y);
        //ropeRenderer.size = new Vector2(ropeRenderer.size.x, height);
        //liftBase.position = liftOrigin;
        leftRope.positionCount = 2;
        leftRope.SetPosition(0, liftBase.position + leftRopeOffsetBase);
        leftRope.SetPosition(1, transform.position + leftRopeOffset);

        rightRope.positionCount = 2;
        rightRope.SetPosition(0, liftBase.position + rightRopeOffsetBase);
        rightRope.SetPosition(1, transform.position + rightRopeOffset);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.GetComponent<PlayerInteractionHandler>() != null)
    //        State = LiftState.Active;
    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerInteractionHandler>() != null)
            StopMoving();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerInteractionHandler>() != null)
        {
            Direction input = GetInput();
            if (direction != input)
            {
                //is moving
                if (input.AsVerticalFloat() != 0)
                {
                    StopMoving();
                    StartMoving(input, collision.transform);
                }
                else
                {
                    StopMoving();
                }
            }
            else
            {
                if (input.AsVerticalFloat() != 0)
                {
                    MovingUpdate(input, collision.transform);
                }
            }
        }
    }

    private void StartMoving(Direction direction, Transform player)
    {
        this.direction = direction;
        State = LiftState.Active;

        playerOffset = player.position - transform.position;
        if (!movingUpAndDownSound.isPlaying)
            movingUpAndDownSound.Play();

        foreach (SpriteAnimator spriteAnimator in wheels_anim)
            spriteAnimator.Play((direction == Direction.Up) ? LiftWheel_active_left : LiftWheel_active_right);

    }

    private void MovingUpdate(Direction direction, Transform player)
    {
        
        distanceJoint.distance -= direction.Inverse().AsVerticalFloat() * Time.deltaTime * liftSpeed;
        player.position = new Vector3(player.position.x, transform.position.y + playerOffset.y);

        movingUpAndDownSound.pitch = 0.75f + 1f * (liftSpeed / liftMaxSpeed);

        foreach (SpriteAnimator spriteAnimator in wheels_anim)
            spriteAnimator.Animation.Speed = (liftSpeed / liftMaxSpeed) * 5;

        if (liftSpeed < liftMaxSpeed)
            liftSpeed += Time.deltaTime * 2f;
    }
    private void StopMoving()
    {
        this.direction = Direction.Right;
        State = LiftState.Inactive;
        liftSpeed = 0;

        if (movingUpAndDownSound.isPlaying)
            movingUpAndDownSound.Pause();

        foreach (SpriteAnimator spriteAnimator in wheels_anim)
            spriteAnimator.Play(LiftWheel_inactive);
    }


    private void Update()
    {
        UpdateCables();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up * 2,liftSpeed.ToString());
    }
#endif

    private Direction GetInput()
    {
        if (Input.GetKey(KeyCode.Q))
            return Direction.Up;
        else if (Input.GetKey(KeyCode.E))
            return Direction.Down;
        else
            return Direction.Right;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + leftRopeOffset, 0.5f);
        Gizmos.DrawWireSphere(transform.position + rightRopeOffset, 0.5f);
        Gizmos.DrawWireSphere(liftBase.position + leftRopeOffsetBase, 0.5f);
        Gizmos.DrawWireSphere(liftBase.position + rightRopeOffsetBase, 0.5f);
    }
}
