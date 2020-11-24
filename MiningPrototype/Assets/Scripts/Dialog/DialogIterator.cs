using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[System.Obsolete]
public class DialogIterator
{
    ProgressionHandler progressionHandler;

    IDialogSection current;
    DialogState state;

    private string topic = "NONE";
    private string payment = "NONE";
    private float amount = -1;

    public IDialogSection CurrentSection { get => current; }
    public DialogState State { get => state; }

    public event System.Action StateChanged;

    public DialogIterator(IDialogSection startSection, ProgressionHandler progressionHandler)
    {
        current = startSection;
        state = DialogState.Answer;
        this.progressionHandler = progressionHandler;
    }

    public string GetCorrectedSentence()
    {
        if (CurrentSection == null)
            return "EMPTY";

        string sentence = current.Sentence;
        sentence = sentence.Replace("{Payment}", payment);
        sentence = sentence.Replace("{Topic}", topic);
        sentence = sentence.Replace("{Amount}", amount.ToString());
        return sentence;
    }

    public void Select(int i)
    {
        if (state == DialogState.Choice)
        {
            SetCurrentTo(current.Choiches[i]);
            switch (current.Consequence)
            {
                case DialogConsequence.JumpBeforeSentence:
                    SetCurrentTo(current.JumpToTarget);
                    break;
            }
            SetStateTo(DialogState.Answer);
        }
    }

    public void Next()
    {
        if (state != DialogState.Answer)
            return;

        switch (current.Consequence)
        {
            case DialogConsequence.Choice:
                SetStateTo(DialogState.Choice);
                break;
            case DialogConsequence.JumpAfterSentence:
                SetCurrentTo(current.JumpToTarget);
                break;

            case DialogConsequence.Exit:
                SetCurrentTo(null);
                break;
            case DialogConsequence.AwaitPayment:
                SetStateTo(DialogState.AwaitPayment);
                break;
        }
    }

    public bool WouldAcceptPaymentWith(ItemAmountPair obj)
    {
        if (state != DialogState.AwaitPayment)
            return false;

        if (payment.ToLowerInvariant() == obj.type.ToString().ToLowerInvariant())
        {
            if (obj.amount >= amount)
            {
                if (InventoryManager.PlayerHas(obj.type, (int)amount))
                {
                    return true;
                }
            }
        }

        return false;

    }

    public void PayWith(ItemAmountPair obj)
    {
        Debug.Log("Payment attempted: " + obj.amount + " " + obj.type.ToString() + " for " + amount + payment);

        if (payment.ToLowerInvariant() == obj.type.ToString().ToLowerInvariant())
        {
            if (obj.amount >= amount)
            {
                if (InventoryManager.PlayerTryPay(obj.type, (int)amount))
                {
                    //Payment recieved;
                    SetCurrentTo(current.JumpToTarget);
                    SetStateTo(DialogState.Answer);

                    //Doing this here is quite dirty!
                    InventoryManager.ForcePlayerInventoryClose();
                    progressionHandler.Aquired(topic);
                    return;
                }
            }
        }

        Debug.LogError("Payment Failed");
    }

    private void SetCurrentTo(IDialogSection section)
    {
        current = section;
        if (current != null)
        {
            if (current.GetTopic() != string.Empty)
                topic = current.GetTopic();

            if (current.GetPayment() != string.Empty)
                payment = current.GetPayment();

            if (topic != "NONE" && payment != "NONE")
            {
                amount = progressionHandler.GetPriceOf(topic, payment);
            }
        }
    }

    private void SetStateTo(DialogState newState)
    {
        state = newState;
        StateChanged?.Invoke();
    }
}
public enum DialogState
{
    Answer,
    Choice,
    AwaitPayment
}

