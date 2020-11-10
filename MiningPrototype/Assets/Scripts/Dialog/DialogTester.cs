using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTester : MonoBehaviour
{
    [SerializeField] string dialogName;
    [SerializeField] Vector2 displayOffset;

    DialogIterator dialog;

    private void Awake()
    {
        dialog = new DialogIterator(DialogParser.GetDialogFromName(dialogName));
    }

    private void OnGUI()
    {
        if (dialog.CurrentSection == null)
            return;

        float y = displayOffset.y;

        if (dialog.State == DialogState.Answer)
        {
            GUI.Box(new Rect(displayOffset.x, y, 200, 50), dialog.GetCorrectedSentence());
            y += 50;
            if (GUI.Button(new Rect(displayOffset.x, y, 200, 30), ".."))
                dialog.Next();
        }
        else if (dialog.State == DialogState.Choice)
        {
            for (int i = 0; i < dialog.CurrentSection.Choiches.Length; i++)
            {
                var choice = dialog.CurrentSection.Choiches[i];
                GUI.color = choice.OptionType == DialogChoiceType.Sentence ? Color.white : Color.yellow;
                if (GUI.Button(new Rect(displayOffset.x, y, 200, 30), choice.OptionText))
                {
                    dialog.Select(i);
                }
                y += 40;
            }
        }
        else if (dialog.State == DialogState.AwaitPayment)
        {
            GUI.Box(new Rect(displayOffset.x, y, 200, 50), "Await Payment");
        }
    }
}
