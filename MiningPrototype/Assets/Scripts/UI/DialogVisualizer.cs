﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogVisualizer : MonoBehaviour, IDialogVisualizer
{
    [Zenject.Inject] InWorldCanvas inWorldCanvas;
    [Zenject.Inject] PlayerManager playerManager;

    [SerializeField] DialogElement dialogElement;
    [SerializeField] Vector3 sentenceOffset, optionsOffset;
    List<DialogElement> dialogElements = new List<DialogElement>();
    event System.Action<int> selectOption;
    public event System.Action<AltarState> OnChangeState;
    public int SentenceCharacterLength = 0;

    public void DisplayOptions(string[] options)
    {
        DialogElement element = InstatiateElement();
        element.StartFollowing(playerManager.GetPlayerTransform(), optionsOffset);
        element.Init(options, this, optionsOffset);
        dialogElements.Add(element);

        OnChangeState?.Invoke(AltarState.Idle);
    }

    public void DisplaySentence(string message)
    {
        dialogElements.Add(InstatiateElement().Init(message, this, sentenceOffset));
        SentenceCharacterLength = message == "..." ? 0 : message.Length;
        OnChangeState?.Invoke(AltarState.Talking);
    }

    private DialogElement InstatiateElement()
    {
        return Instantiate(dialogElement, transform.position, Quaternion.identity, inWorldCanvas.transform);
    }

    public void EndDialog()
    {
        OnChangeState?.Invoke(AltarState.Passive);
        Clear();
    }

    public void StartDialog()
    {
        OnChangeState?.Invoke(AltarState.Idle);
    }
    public void Clear()
    {
        for (int i = dialogElements.Count - 1; i >= 0; i--)
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
