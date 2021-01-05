using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogVisualizer : MonoBehaviour, IDialogVisualizer
{
    [Zenject.Inject] InWorldCanvas inWorldCanvas;
    [Zenject.Inject] PlayerInteractionHandler playerInteractionHandler;

    [SerializeField] DialogElement dialogElement;
    [SerializeField] Vector3 sentenceOffset, optionsOffset;
    List<DialogElement> dialogElements = new List<DialogElement>();
    event System.Action<int> selectOption;

    public void DisplayOptions(string[] options)
    {
        DialogElement element = InstatiateElement();
        element.StartFollowing(playerInteractionHandler.transform, optionsOffset);
        element.Init(options, this, optionsOffset);
        dialogElements.Add(element);
    }

    public void DisplaySentence(string message)
    {
        dialogElements.Add(InstatiateElement().Init(message, this, sentenceOffset));
    }

    private DialogElement InstatiateElement()
    {
        return Instantiate(dialogElement, transform.position, Quaternion.identity, inWorldCanvas.transform);
    }

    public void EndDialog()
    {
        Clear();
    }

    public void StartDialog()
    {
        //
    }
    public void Clear()
    {
        for (int i = dialogElements.Count-1; i >= 0; i--)
        {
            dialogElements[i].Hide();
        }
    }

    public void SubscribeToSelection(Action<int> a)
    {
        selectOption += a;
    }

    public void UnsubscribeFromSelection(Action<int> a)
    {
        selectOption -= a;
    }

    public void OnSelectOption(int optionSelected)
    {
        Clear();
        selectOption?.Invoke(optionSelected);
    }
}
