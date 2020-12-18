using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public enum LiftState
{
    Active,
    Inactive
}

public class LiftCage : MonoBehaviour
{
    [SerializeField] LiftState state = LiftState.Inactive;
    [BoxGroup("Positions")] [SerializeField] Vector3 liftOrigin;
    [BoxGroup("Positions")] [SerializeField] Vector3 leftRopeOffset, rightRopeOffset, leftRopeOffsetBase, rightRopeOffsetBase;
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

    

    float liftSpeed = 0;
    [SerializeField] float liftMaxSpeed = 10;
    [BoxGroup("Animations")][SerializeField] SpriteAnimator liftFG_anim, liftBG_anim;
    [BoxGroup("Animations")] [SerializeField] SpriteAnimation liftFG_active, liftFG_inactive, liftBG_active, LiftBG_inactive;
    [SerializeField] SpriteRenderer ropeRenderer;
    [SerializeField] Transform liftBase;
    [SerializeField] LineRenderer leftRope, rightRope;
    [SerializeField] DistanceJoint2D distanceJoint;
    private void Start()
    {
        liftOrigin = transform.position + Vector3.up * 3 + Vector3.left * 0.75f;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerInteractionHandler>() != null)
            State = LiftState.Active;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerInteractionHandler>() != null)
            State = LiftState.Inactive;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerInteractionHandler>() != null)
        {
            Vector3 input = GetInput();
            collision.transform.Translate(input * Time.deltaTime * liftSpeed);
            distanceJoint.distance -= input.y * Time.deltaTime * liftSpeed;

            if (input != Vector3.zero && liftSpeed < liftMaxSpeed)
                liftSpeed += Time.deltaTime * 6f;
        }
    }

    private void Update()
    {
        if (liftSpeed > 0)
            liftSpeed -= Time.deltaTime*5f;

        UpdateCables();
    }

    private Vector3 GetInput()
    {
        if (Input.GetKey(KeyCode.Q))
            return Vector3.up;
        else if (Input.GetKey(KeyCode.E))
            return Vector3.down;
        else
            return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + leftRopeOffset, 0.5f);
        Gizmos.DrawWireSphere(transform.position + rightRopeOffset, 0.5f);
        Gizmos.DrawWireSphere(liftOrigin + leftRopeOffsetBase, 0.5f);
        Gizmos.DrawWireSphere(liftOrigin + rightRopeOffsetBase, 0.5f);
    }
}
