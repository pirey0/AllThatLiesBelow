using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] SpriteAnimator bodyAnimator;
    [SerializeField] HeadAnimator headAnimator;
    [SerializeField] SpriteRenderer handsRenderer;
    [SerializeField] SpriteRenderer itemRenderer;
    [SerializeField] PickaxeAnimator pickaxe;
    [SerializeField] GameObject pickaxeObject;
    [SerializeField] SpriteRenderer pickaxeRenderer;
    [SerializeField] PlayerWalkEffects playerWalkEffects;

    [SerializeField] PlayerVisualState[] visualStates;
    [SerializeField] SpriteAnimation[] climbNoHelmet, climbIdleNoHelmet, climbHelmet, climbIdleHelmet, climbHelmetLamp, climbIdleHelmetLamp;
    [SerializeField] HelmetAndPickaxeBasedAnimationHolder[] helmetAndPickaxeBasedAnimationHolders;

    [Zenject.Inject] ProgressionHandler progressionHandler;

    Dictionary<string, PlayerVisualState> visualStateMap;

    BasePlayerStateMachine player;
    StateMachine stateMachine;
    StateMachine.State prevOldState, prevNewState;

    HelmetState helmentState;
    int pickaxeLevel = 1;
    int pickaxeMaxLevel = 4;

    private void Start()
    {
        visualStateMap = new Dictionary<string, PlayerVisualState>();

        foreach (var item in visualStates)
        {
            visualStateMap.Add(item.StateName, item);
        }

        var smUser = GetComponent<BasePlayerStateMachine>();
        if (smUser != null)
        {
            stateMachine = smUser.GetStateMachine();
            stateMachine.StateChanged += VisualUpdate;
            player = smUser;
        }
        else
        {
            Debug.LogError("No StateMachine found");
        }
    }

    private void OnEnable()
    {
        progressionHandler.OnChangePickaxeLevel += ChangePickaxeLevel;
    }

    private void OnDisable()
    {
        progressionHandler.OnChangePickaxeLevel -= ChangePickaxeLevel;
    }

    public void ChangeHelmetState(HelmetState newHelmentState)
    {
        headAnimator.ChangeHelmetState(newHelmentState);
        helmentState = newHelmentState;
        UpdateClimbAnimations();
    }

    public void ChangePickaxeLevel(int newLevel)
    {
        pickaxeLevel = Mathf.Clamp(newLevel,1, pickaxeMaxLevel);
        pickaxe.SetPickaxeLevel(pickaxeLevel);
        UpdateClimbAnimations();
    }

    [Button]
    public void AddHelmet()
    {
        ChangeHelmetState(HelmetState.Helmet);
    }

    [Button]
    public void AddHelmetWithLamp()
    {
        ChangeHelmetState(HelmetState.HelmetWidthLamp);
    }

    [Button]
    public void RemoveHelmet()
    {
        ChangeHelmetState(HelmetState.None);
    }

    [Button]
    public void IncreasePickaxeState()
    {
        ChangePickaxeLevel(pickaxeLevel + 1);
    }

    [Button]
    public void DecreasePickaxeState()
    {
        ChangePickaxeLevel(pickaxeLevel - 1);
    }

    private void UpdateClimbAnimations()
    {
        visualStateMap["Climb"].BodyAnimation = GetBodyAnimationFor("Climb");
        visualStateMap["ClimbIdle"].BodyAnimation = GetBodyAnimationFor("ClimbIdle");
        visualStateMap["LongIdle"].BodyAnimation = GetBodyAnimationFor("LongIdle");
    }

    private SpriteAnimation GetBodyAnimationFor(string statename)
    {
        if (statename == "ClimbIdle")
        {
            switch (helmentState)
            {
                case HelmetState.Helmet:
                    return climbIdleHelmet[pickaxeLevel - 1];

                case HelmetState.HelmetWidthLamp:
                    return climbIdleHelmetLamp[pickaxeLevel - 1];
            }

            return climbIdleNoHelmet[pickaxeLevel - 1];
        } else if (statename == "LongIdle")
        {
            foreach (HelmetAndPickaxeBasedAnimationHolder holder in helmetAndPickaxeBasedAnimationHolders)
            {
                if (holder.helmetState == helmentState)
                    return holder.animations[pickaxeLevel - 1];
            }
        }

        switch (helmentState)
        {
            case HelmetState.Helmet:
                return climbHelmet[pickaxeLevel - 1];

            case HelmetState.HelmetWidthLamp:
                return climbHelmetLamp[pickaxeLevel - 1];
        }

        return climbNoHelmet[pickaxeLevel - 1];
    }

    public void ForceUpdate()
    {
        VisualUpdate(prevOldState, prevNewState);
    }

    private void VisualUpdate(StateMachine.State leavingState, StateMachine.State enteringState)
    {
        prevOldState = leavingState;
        prevNewState = enteringState;

        if (visualStateMap.ContainsKey(enteringState.Name))
        {
            var visState = visualStateMap[enteringState.Name];

            bodyAnimator.Play(visState.BodyAnimation, resetSame: false);

            switch (visState.CarryItemState)
            {
                case CarryItemState.Hidden:
                    itemRenderer.enabled = false;
                    break;

                case CarryItemState.Conditional:
                    itemRenderer.enabled = true;
                    itemRenderer.sortingOrder = 5;
                    break;

                case CarryItemState.ConditionalBehindPlayer:
                    itemRenderer.enabled = true;
                    itemRenderer.sortingOrder = -5;
                    break;
            }

            if (headAnimator != null)
            {
                switch (visState.HeadState)
                {
                    case HeadState.Integrated:
                        headAnimator.enabled = false;
                        break;

                    case HeadState.Dynamic:
                        headAnimator.enabled = true;
                        break;
                }
            }

            switch (visState.HandsState)
            {
                case HandsState.Integrated:
                    handsRenderer.enabled = false;
                    break;
                case HandsState.Conditional:
                    if (player.ShouldHoldPickaxe())
                        handsRenderer.enabled = false;
                    else
                        handsRenderer.enabled = true;
                    break;
            }

            switch (visState.PickaxeState)
            {
                case AnimationPickaxeState.Integrated:
                    pickaxeObject.SetActive(false);
                    break;

                case AnimationPickaxeState.Behind:
                    pickaxeObject.SetActive(true);
                    pickaxeRenderer.sortingOrder = -10;
                    break;

                case AnimationPickaxeState.Conditional:
                    if (player.ShouldHoldPickaxe())
                    {
                        pickaxeRenderer.sortingOrder = 10;
                        pickaxeObject.SetActive(true);
                    }
                    else
                    {
                        pickaxeRenderer.sortingOrder = -10;
                        pickaxeObject.SetActive(true);
                    }
                    break;
            }

            //show walk effects if the player is walking on the ground while not beeing disabled
            if (playerWalkEffects != null)
                playerWalkEffects.SetEffects(player.IsMoving() && player.IsGrounded() && visState.StateName != "Disabled");
        }
        else
        {
            Debug.LogError("Undefined visuals state: " + visualStateMap);
        }
    }
}

public enum HelmetState
{
    None,
    Helmet,
    HelmetWidthLamp
}

[System.Serializable]
public class HelmetAndPickaxeBasedAnimationHolder
{
    public HelmetState helmetState;
    public SpriteAnimation[] animations;
}
