using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] SpriteAnimator bodyAnimator;
    [SerializeField] HeadAnimator headAnimator;
    [SerializeField] SpriteRenderer handsRenderer;
    [SerializeField] SpriteRenderer itemRenderer;
    [SerializeField] DirectionBasedAnimator pickaxe;
    [SerializeField] GameObject pickaxeObject;

    [SerializeField] PlayerVisualState[] visualStates;


    Dictionary<string, PlayerVisualState> visualStateMap;


    StateMachine stateMachine;

    private void Start()
    {
        visualStateMap = new Dictionary<string, PlayerVisualState>();

        foreach (var item in visualStates)
        {
            visualStateMap.Add(item.StateName, item);
        }

        var smUser = GetComponent<IStateMachineUser>();
        if (smUser != null)
        {
            stateMachine = smUser.GetStateMachine();
            stateMachine.StateChanged += VisualUpdate;
        }
        else
        {
            Debug.LogError("No StateMachine found");
        }

    }

    private void VisualUpdate(StateMachine.State leavingState, StateMachine.State enteringState)
    {
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
                    break;

                case CarryItemState.ConditionalBehindPlayer:
                    itemRenderer.enabled = true;
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
                    break;

                case AnimationPickaxeState.Conditional:
                    pickaxeObject.SetActive(true);
                    break;
            }

        }
        else
        {
            Debug.LogError("Undefined visuals state: " + visualStateMap);
        }
    }
}
