using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IDropReceiver
{
    bool WouldTakeDrop(ItemAmountPair pair);
    void BeginHoverWith(ItemAmountPair pair);
    void EndHover();
    void HoverUpdate(ItemAmountPair pair);
    void ReceiveDrop(ItemAmountPair pair);
}

public enum AltarState
{
    Off,
    Intro,
    RewardSelection,
    AwaitPayment,
    PaymentAccepted,
    PaymentInsufficient,
    PaymentRefused
}

public class Altar : MonoBehaviour, IInteractable, IDropReceiver
{
    [SerializeField] Transform cameraTarget;
    [SerializeField] AltarDialogVisualizer visualizer;
    [SerializeField] string testDialog;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Material spriteDefault, spriteOuline;

    [Inject] ProgressionHandler progressionHandler;
    [Inject] CameraController cameraController;

    AltarState currentState = AltarState.Off;
    string[] availableRewards;
    string selectedReward;
    ItemAmountPair[] acceptedPayments;

    private event System.Action NotifyForcedEnd;

    private bool InInteraction { get => currentState != AltarState.Off; }

    public void BeginInteracting(GameObject interactor)
    {
        gameObject.layer = 12;
        visualizer.Progressed += OnProgressed;
        if (progressionHandler.DailySacrificeExpired)
            ChangeStateTo(AltarState.PaymentAccepted);
        else
            ChangeStateTo(AltarState.Intro);
    }

    private void OnProgressed(int index)
    {
        if (!InInteraction)
            return;

        switch (currentState)
        {
            case AltarState.Intro:
                availableRewards = SacrificePricesParser.GetRewardsAvailableAtLevel(progressionHandler.SacrificeProgressionLevel);
                ChangeStateTo(AltarState.RewardSelection);
                break;

            case AltarState.RewardSelection:
                selectedReward = availableRewards[index];
                acceptedPayments = SacrificePricesParser.GetPaymentsFor(selectedReward);
                ChangeStateTo(AltarState.AwaitPayment);
                break;

            case AltarState.PaymentRefused:
            case AltarState.PaymentInsufficient:
                ChangeStateTo(AltarState.AwaitPayment);
                break;

            case AltarState.PaymentAccepted:
                ChangeStateTo(AltarState.Off);
                break;
        }

    }

    public void EndInteracting(GameObject interactor)
    {
        Debug.Log("End Altar Interaction");
        visualizer.Progressed -= OnProgressed;
        visualizer.EndDialog();
        cameraController.TransitionToDefault();
        gameObject.layer = 0;
        ChangeStateTo(AltarState.Off);
    }

    private void ChangeStateTo(AltarState newState)
    {

        switch (newState)
        {
            case AltarState.Intro:
                visualizer.StartDialog();
                cameraController.TransitionToNewTarget(cameraTarget);
                visualizer.DisplaySentence("What do you desire?");
                break;

            case AltarState.RewardSelection:

                visualizer.DisplayOptions(availableRewards);
                break;

            case AltarState.AwaitPayment:

                visualizer.DisplaySentence("And how will you pay?");
                break;

            case AltarState.PaymentAccepted:
                visualizer.DisplaySentence("Seek your reward in the morning");
                break;

            case AltarState.PaymentInsufficient:
                visualizer.DisplaySentence("That will take more..");
                break;

            case AltarState.PaymentRefused:
                visualizer.DisplaySentence("that will not work");
                break;

        }

        currentState = newState;
    }

    public void SubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd -= action;
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {

        if(currentState == AltarState.AwaitPayment)
        {
            if(acceptedPayments == null ||acceptedPayments.Length == 0)
            {
                Debug.LogError("No accepted payments for " + selectedReward);
                return false;
            }

            return true;
        }

        return false;

    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        if (!WouldTakeDrop(pair))
            return;

        spriteRenderer.material = spriteOuline;
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f);
    }

    public void EndHover()
    {
        spriteRenderer.material = spriteDefault;
        spriteRenderer.color = Color.white;
    }

    public void ReceiveDrop(ItemAmountPair pair)
    {
        spriteRenderer.color = Color.grey;

        if (IsValidItemToPayWith(pair))
        {
            if (IsEnoughToPayWith(pair))
            {

                Debug.Log("Received drop of " + pair.type);
                InventoryManager.PlayerTryPay(pair.type, pair.amount);

                ItemAmountPair payment = ItemAmountPair.Nothing;
                for (int i = 0; i < acceptedPayments.Length; i++)
                {
                    if (acceptedPayments[i].type == pair.type)
                        payment = acceptedPayments[i];
                }

                ChangeStateTo(AltarState.PaymentAccepted);
                progressionHandler.Aquired(selectedReward, payment);
            }
            else
            {
                ChangeStateTo(AltarState.PaymentInsufficient);
            }
        } else
        {
            ChangeStateTo(AltarState.PaymentRefused);
        }
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //if (WouldTakeDrop(pair))
        //{
        //    overlay.color = Color.green;
        //}
        //else
        //{
        //    overlay.color = Color.red;
        //}
    }

    private bool IsValidItemToPayWith(ItemAmountPair pair)
    {
        for (int i = 0; i < acceptedPayments.Length; i++)
        {
            if (acceptedPayments[i].type == pair.type)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsEnoughToPayWith(ItemAmountPair pair)
    {
        for (int i = 0; i < acceptedPayments.Length; i++)
        {
            if (acceptedPayments[i].type == pair.type)
            {
                if (acceptedPayments[i].amount <= pair.amount)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
