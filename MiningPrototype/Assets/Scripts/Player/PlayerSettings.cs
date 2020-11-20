using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PlayerSettings")]
public class PlayerSettings : ScriptableObject
{

    public float groundedAngle;
    public float jumpVelocity;
    public float moveSpeed;
    public float slowMoveSpeed;
    public float timeToLongIdle;
    public float hitDuration;
    public float respawnCooldown = 5;
    
    public float jumpCooldown = 0.1f;
    public float timeAfterGroundedToJump = 0.1f;
    
    public float feetRadius;
    
    public float maxDigDistance = 3;
    
    public float digSpeed = 10;
    
    public SpriteAnimation an_Walk, an_Idle, an_Fall, an_Inventory, an_Climb, an_ClimbIdle;
    
    public int miningBreakParticlesCount;
    public float miningParticlesRateOverTime = 4;
    
    public float inventoryOpenDistance;
    public float maxInteractableDistance;
    
    public float climbSpeed;
    public float climbPanSpeed;
    public float climbIdleThreshold;
    public float idleThreshold;

}
