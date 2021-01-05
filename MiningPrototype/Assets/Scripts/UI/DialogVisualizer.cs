using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogVisualizer : MonoBehaviour, IDialogVisualizer
{
    [Zenject.Inject] InWorldCanvas inWorldCanvas;
    [SerializeField] DialogElement dialogElement;
    [SerializeField] Vector3 sentenceOffset, optionsOffset;
    List<DialogElement> dialogElements = new List<DialogElement>();

    public void DisplayOptions(string[] options)
    {
        dialogElements.Add(InstatiateElement(optionsOffset).Init(options, this));
    }

    public void DisplaySentence(string message)
    {
        dialogElements.Add(InstatiateElement(sentenceOffset).Init(message));
    }

    private DialogElement InstatiateElement(Vector3 offset)
    {
        return Instantiate(dialogElement, transform.position + offset, Quaternion.identity, inWorldCanvas.transform);
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
        Destroy(gameObject);
    }

    public void SubscribeToSelection(Action<int> a)
    {
        a += SelectOption;
    }

    public void UnsubscribeFromSelection(Action<int> a)
    {
        a += SelectOption;
    }

    public void SelectOption(int optionSelected)
    {

    }
}
