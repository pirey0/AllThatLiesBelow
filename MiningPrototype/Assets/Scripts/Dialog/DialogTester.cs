using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTester : MonoBehaviour
{
    [SerializeField] string dialogName;
    [SerializeField] Vector2 displayOffset;

    IDialogSection current;
    DialogState state;

    private void Awake()
    {
        current = DialogParser.GetDialogFromName(dialogName);
    }

    private void OnGUI()
    {
        if (current == null)
            return;

        float y = displayOffset.y;

        if (state == DialogState.Answer)
        {
            GUI.Box(new Rect(displayOffset.x, y, 200, 50), current.Sentence);
            y += 50;
            if (GUI.Button(new Rect(displayOffset.x, y, 200, 30), ".."))
                Next();
        }
        else if (state == DialogState.Choice)
        {
            for (int i = 0; i < current.Choiches.Length; i++)
            {
                var choice = current.Choiches[i];
                GUI.color = choice.OptionType == DialogChoiceType.Sentence ? Color.white : Color.yellow;
                if (GUI.Button(new Rect(displayOffset.x, y, 200, 30), choice.OptionText))
                {
                    Select(i);
                }
                y += 40;
            }
        }
        else if (state == DialogState.AwaitPayment)
        {
            GUI.Box(new Rect(displayOffset.x, y, 200, 50), "Await Payment");
        }

    }

    private void Select(int i)
    {
        current = current.Choiches[i];
        state = DialogState.Answer;
        switch (current.Consequence)
        {
            case DialogConsequence.JumpBeforeSentence:
                current = current.JumpToTarget;
                break;
        }
    }

    private void Next()
    {
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
    }
}
public enum DialogState
{
    Answer,
    Choice,
    AwaitPayment
}

