using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IDropReceiver
{
    bool WouldTakeDrop(ItemAmountPair pair);
    void BeginHoverWith(ItemAmountPair pair);
    void EndHover();
    void HoverUpdate(ItemAmountPair pair);
    void ReceiveDrop(ItemAmountPair pair, Inventory origin);
}

public enum AltarState
{
    Off,
    Intro,
    RewardSelection,
    AskForPayment,
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
    [Inject] SacrificePricesParser sacrificePricesParser;

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
                availableRewards = sacrificePricesParser.GetRewardsAvailableAtLevel(progressionHandler.SacrificeProgressionLevel, progressionHandler.RewardsReceived);



                ChangeStateTo(AltarState.RewardSelection);
                break;

            case AltarState.RewardSelection:
                selectedReward = availableRewards[index];
                acceptedPayments = sacrificePricesParser.GetPaymentsFor(selectedReward);
                ChangeStateTo(AltarState.AskForPayment);
                break;

            case AltarState.AskForPayment:
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
                string[] rewardsContent = new string[availableRewards.Length];
                
                for (int i = 0; i < availableRewards.Length; i++)
                    rewardsContent[i] = sacrificePricesParser.GetDisplayNameOf(availableRewards[i]);

                visualizer.DisplayOptions(rewardsContent);
                break;

            case AltarState.AskForPayment:

                visualizer.DisplaySentence("And how will you pay?");
                break;

            case AltarState.PaymentAccepted:
                visualizer.DisplaySentence("Seek your reward in the morning");
                break;

            case AltarState.PaymentInsufficient:
                visualizer.DisplaySentence("That will take more..");
                break;

            case AltarState.PaymentRefused:
                visualizer.DisplaySentence(SuggestPossiblePaymentMethods());
                break;

        }

        currentState = newState;
    }

    public string SuggestPossiblePaymentMethods()
    {
        List<string> paymentWords = new List<string>();

        foreach (ItemAmountPair payment in acceptedPayments)
        {
            string[] pw = ItemsData.GetItemInfo(payment.type).AltarWords;

            if (pw != null && pw.Length > 0)
                paymentWords.AddRange(pw);
        }

        string str = Util.ChooseRandomString("What about ", "Give me ", "I want ");

        paymentWords = paymentWords.OrderBy(a => Guid.NewGuid()).ToList(); //random shuffle

        for (int i = 0; i < (Mathf.Min(5, paymentWords.Count)); i++)
        {
            str += paymentWords[i] + " ";
        }

        return str;
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

        if(currentState == AltarState.AwaitPayment || currentState == AltarState.AskForPayment)
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

    public void ReceiveDrop(ItemAmountPair pair, Inventory inventoryPaidFrom)
    {
        spriteRenderer.color = Color.grey;

        if (IsValidItemToPayWith(pair))
        {
            if (IsEnoughToPayWith(pair))
            {

                Debug.Log("Received drop of " + pair.type);

                ItemAmountPair payment = ItemAmountPair.Nothing;
                for (int i = 0; i < acceptedPayments.Length; i++)
                {
                    if (acceptedPayments[i].type == pair.type)
                        payment = acceptedPayments[i];
                }

                ChangeStateTo(AltarState.PaymentAccepted);
                progressionHandler.Aquired(selectedReward, payment);
                inventoryPaidFrom.TryRemove(payment);
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
