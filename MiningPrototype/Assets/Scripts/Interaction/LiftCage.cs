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

public class LiftCage : MonoBehaviour, IVehicle
{

    [BoxGroup("Positions")] [SerializeField] Vector3 liftOrigin;
    [BoxGroup("Positions")] [SerializeField] Vector3 leftRopeOffset, rightRopeOffset, leftRopeOffsetBase, rightRopeOffsetBase;

    [BoxGroup("Animations")] [SerializeField] SpriteAnimator liftFG_anim, liftBG_anim;
    [BoxGroup("Animations")] [SerializeField] SpriteAnimator[] wheels_anim;
    [BoxGroup("Animations")] [SerializeField] SpriteAnimation liftFG_active, liftFG_inactive, liftBG_active, LiftBG_inactive, LiftWheel_active_left, LiftWheel_active_right, LiftWheel_inactive;
    [SerializeField] SpriteRenderer ropeRenderer;
    [SerializeField] Transform liftBase;
    [SerializeField] LineRenderer leftRope, rightRope, centerRope;
    [SerializeField] DistanceJoint2D distanceJoint;
    [SerializeField] AudioSource movingUpAndDownSound, engineSound, startSound, stopSound;
    [SerializeField] ParticleSystem smokeParticles;
    [SerializeField] Lift lift;

    [Header("Settings")]
    [SerializeField] float acceleration = 2;
    [SerializeField] float decelerationMultiplyer = 2;
    [SerializeField] float maxSpeed = 4;
    [SerializeField] float minLength = 1, maxLength = 30;

    [Zenject.Inject] CameraController cameraController;

    LiftState state = LiftState.Inactive;
    PlayerStateMachine player;
    Vector3 oldPosition;
    ParentedCameraShake cameraShake;
    float liftVelocity = 0;

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

    private void Start()
    {
        liftOrigin = transform.position + Vector3.up * 3 + Vector3.left * 0.75f;
    }

    private void AdaptVisualsToState(LiftState state)
    {
        liftFG_anim.Play((state == LiftState.Active) ? liftFG_active : liftFG_inactive);
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

        centerRope.positionCount = 2;
        centerRope.SetPosition(0, liftBase.position);
        centerRope.SetPosition(1, transform.position + new Vector3(distanceJoint.anchor.x, distanceJoint.anchor.y));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerStateMachine psm))
        {
            if (!psm.InVehicle())
            {
                this.player = psm;
                psm.EnterVehicle(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerStateMachine psm))
        {
            psm.ExitVehicle(this);
            this.player = null;
        }
    }

    private IEnumerator MoveRoutine()
    {
        lift.RecalcuateHeight();
        maxLength = lift.GetHeight();

        while (!Util.IsNullOrDestroyed(player) || liftVelocity != 0)
        {
            Direction input = GetInput().Inverse();

            if (state == LiftState.Inactive && input != Direction.None)
            {
                State = LiftState.Active;
                Debug.Log("YO: " + input);
                if (input == Direction.Up) //Recalculate max height when starting to move down
                {
                    lift.RecalcuateHeight();
                    maxLength = lift.GetHeight();
                }

            }
            else if (state == LiftState.Active && input == Direction.None)
            {
                State = LiftState.Inactive;
            }

            MovingUpdate(input);
            yield return null;
        }
    }

    private void MovingUpdate(Direction direction)
    {
        //Movement
        float vert = direction.AsVerticalFloat();
        if (state == LiftState.Active && vert != 0)
        {
            if (liftVelocity * vert < maxSpeed)
                liftVelocity += vert * Time.deltaTime * acceleration;
        }
        else
        {
            liftVelocity = Mathf.Lerp(liftVelocity, 0, Time.deltaTime * decelerationMultiplyer);
            if (Mathf.Abs(liftVelocity) < 0.005f)
                liftVelocity = 0;
        }

        var main = smokeParticles.main;
        var emit = smokeParticles.emission;
        float speedPercent = (Mathf.Abs(liftVelocity) / maxSpeed);

        //Udate Visuals and sound
        if (speedPercent < 0.1f)
        {
            if (movingUpAndDownSound.isPlaying)
            {
                movingUpAndDownSound.Pause();
                engineSound.Pause();
                stopSound.Play();
                foreach (SpriteAnimator spriteAnimator in wheels_anim)
                    spriteAnimator.Play(LiftWheel_inactive);

                emit.rateOverTimeMultiplier = 0;
                emit.rateOverDistanceMultiplier = 0;
                cameraController.StopShake(cameraShake);
                cameraShake = null;
            }
        }
        else
        {
            if (!movingUpAndDownSound.isPlaying)
            {
                movingUpAndDownSound.Play();
                engineSound.Play();
                startSound.Play();
                foreach (SpriteAnimator spriteAnimator in wheels_anim)
                    spriteAnimator.Play((direction == Direction.Up) ? LiftWheel_active_left : LiftWheel_active_right);

                if (cameraShake != null)
                {
                    cameraController.StopShake(cameraShake);
                }
                cameraShake = cameraController.ParentedShake(transform, 10, 1);
            }

            movingUpAndDownSound.pitch = 0.75f + 0.75f * speedPercent;
            engineSound.pitch = 0.75f + 0.5f * speedPercent;

            emit.rateOverTimeMultiplier = speedPercent * 10;
            emit.rateOverDistanceMultiplier = speedPercent * 10;
            main.simulationSpeed = 0.75f + speedPercent * 0.5f;
            cameraShake.SetIntensity(speedPercent);
        }

        //Update distanceJoint
        distanceJoint.distance += Time.deltaTime * liftVelocity;

        if (distanceJoint.distance < minLength)
        {
            distanceJoint.distance = minLength;
            liftVelocity = 0;
        }
        else if (distanceJoint.distance > maxLength)
        {
            distanceJoint.distance = maxLength;
            liftVelocity = 0;
        }


        //Update animationspeed
        foreach (SpriteAnimator spriteAnimator in wheels_anim)
            spriteAnimator.Animation.Speed = speedPercent * 5;

        //Move player to avoid collision
        Vector2 diff = transform.position - oldPosition;
        if (player != null)
            player.Rigidbody.position += diff;

        oldPosition = transform.position;
    }

    private void Update()
    {
        UpdateCables();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up * 2, liftVelocity.ToString());
    }
#endif

    private Direction GetInput()
    {
        if (player == null)
            return Direction.None;
        float vert = player.GetVerticalInputRaw();

        if (vert > 0)
            return Direction.Up;
        else if (vert < 0)
            return Direction.Down;
        else
            return Direction.None;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + leftRopeOffset, 0.5f);
        Gizmos.DrawWireSphere(transform.position + rightRopeOffset, 0.5f);
        Gizmos.DrawWireSphere(liftBase.position + leftRopeOffsetBase, 0.5f);
        Gizmos.DrawWireSphere(liftBase.position + rightRopeOffsetBase, 0.5f);
    }

    public bool ConsumesVerticalInput()
    {
        return true;
    }

    public bool ConsumesHorizontalInput()
    {
        return false;
    }

    public void EnteredBy(PlayerStateMachine player)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine());
        player.transform.parent = transform;
        Debug.Log("Player entered " + this.name);
    }

    public void LeftBy(PlayerStateMachine player)
    {

        player.transform.parent = null;
        Debug.Log("Player left " + this.name);
    }

    public void SaveTo(Lift.LiftSaveData data)
    {
        data.CagePosition = new SerializedVector3(transform.position);
        data.CageDistance = distanceJoint.distance;
        data.CageVelocity = liftVelocity;
        data.CageState = state;
    }

    public void Load(Lift.LiftSaveData data)
    {
        transform.position = data.CagePosition.ToVector3();
        distanceJoint.distance = data.CageDistance;
        liftVelocity = data.CageVelocity;
        state = data.CageState;

        if (state == LiftState.Active || liftVelocity != 0)
        {
            StartCoroutine(MoveRoutine());
        }
    }
}
