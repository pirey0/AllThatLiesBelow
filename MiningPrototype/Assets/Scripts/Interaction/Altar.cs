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
    bool IsSameInventory(Inventory inventory);
}

public class Altar : StateListenerBehaviour, IInteractable, IDialogUser
{
    INodeServiceProvider dialogServices;
    AltarBaseNode startingNode;
    private bool inInteraction;
    private event System.Action<IInteractable> NotifyForcedEnd;

    public void BeginInteracting(GameObject interactor)
    {
        dialogServices.Aborted = false;
        Debug.Log("Begin Altar Interaction");
        gameObject.layer = 12;
        inInteraction = true;

        StartCoroutine(AltarDialogRunner.DialogCoroutine(dialogServices, startingNode, onDialogFinished));
    }

    private void onDialogFinished()
    {
        NotifyForcedEnd?.Invoke(this);
    }

    public void EndInteracting(GameObject interactor)
    {
        Debug.Log("End Altar Interaction");
        gameObject.layer = 0;
        dialogServices.Aborted = true;
        inInteraction = false;
    }

    public void SubscribeToForceQuit(Action<IInteractable> action)
    {
        NotifyForcedEnd += action;
    }

    public void UnsubscribeToForceQuit(Action<IInteractable> action)
    {
        NotifyForcedEnd -= action;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inInteraction && collision.TryGetComponent<PlayerInteractionHandler>(out var pi))
        {
            pi.ForceInteractionWith(this);
        }
    }

    public void Setup(INodeServiceProvider services, AltarBaseNode node)
    {
        dialogServices = services;
        startingNode = node;
    }
}
