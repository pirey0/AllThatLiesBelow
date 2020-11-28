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
    [SerializeField] GameObject sweatParticles;

    [SerializeField] PlayerVisualState[] visualStates;


    Dictionary<string, PlayerVisualState> visualStateMap;

    PlayerStateMachine player;
    StateMachine stateMachine;
    StateMachine.State prevOldState, prevNewState;

    private void Start()
    {
        visualStateMap = new Dictionary<string, PlayerVisualState>();

        foreach (var item in visualStates)
        {
            visualStateMap.Add(item.StateName, item);
        }

        var smUser = GetComponent<PlayerStateMachine>();
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

            switch (visState.HeadState)
            {
                case HeadState.Integrated:
                    headAnimator.enabled = false;
                    break;

                case HeadState.Dynamic:
                    headAnimator.enabled = true;
                    break;
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

        }
        else
        {
            Debug.LogError("Undefined visuals state: " + visualStateMap);
        }

        if (leavingState.Name == "SlowWalk")
            sweatParticles.SetActive(false);

        if (enteringState.Name == "SlowWalk")
            sweatParticles.SetActive(true);
    }
}
