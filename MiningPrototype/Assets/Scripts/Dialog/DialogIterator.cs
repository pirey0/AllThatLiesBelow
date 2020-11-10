using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogIterator
{
    public DialogIterator(IDialogSection startSection)
    {
        current = startSection;
        state = DialogState.Answer;
    }

    IDialogSection current;
    DialogState state;

    public IDialogSection CurrentSection { get => current; }
    public DialogState State { get => state; }

    public event System.Action StateChanged;

    public void Select(int i)
    {
        if(state == DialogState.Choice)
        {
            current = current.Choiches[i];
            state = DialogState.Answer;
            switch (current.Consequence)
            {
                case DialogConsequence.JumpBeforeSentence:
                    current = current.JumpToTarget;
                    break;
            }
            StateChanged?.Invoke();
        }
    }

    public void Next()
    {
        if (state != DialogState.Answer)
            return;

        switch (current.Consequence)
        {
            case DialogConsequence.Choice:
                state = DialogState.Choice;
                break;
            case DialogConsequence.JumpAfterSentence:
                current = current.JumpToTarget;
                break;

            case DialogConsequence.Exit:
                current = null;
                break;
            case DialogConsequence.AwaitPayment:
                state = DialogState.AwaitPayment;
                break;
        }

        StateChanged?.Invoke();
    }
}
public enum DialogState
{
    Answer,
    Choice,
    AwaitPayment
}

