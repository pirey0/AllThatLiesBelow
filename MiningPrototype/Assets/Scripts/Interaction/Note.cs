using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MirrorWorldFollower, IInteractable
{
    [SerializeField] string text;

    [Zenject.Inject] ReadableItemHandler readableItemHandler;

    int id;
    private event System.Action forceQuit;

    private void Start()
    {
        id = readableItemHandler.AddNewReadable(text);
    }

    public void BeginInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent += OnHide;
        readableItemHandler.Display(id, null);
    }

    private void OnHide()
    {
        forceQuit?.Invoke();
    }

    public void EndInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent -= OnHide;
        readableItemHandler.Hide();
        Destroy(gameObject);
    }

    public void SubscribeToForceQuit(Action action)
    {
        forceQuit += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        forceQuit -= action;
    }
}
