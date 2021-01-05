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
    InProgress
}

public class Altar : StateListenerBehaviour, IInteractable, IDialogUser
{
    INodeServiceProvider dialogServices;
    AltarBaseNode startingNode;

    private event System.Action NotifyForcedEnd;

    public void BeginInteracting(GameObject interactor)
    {
        dialogServices.Aborted = false;
        Debug.Log("Begin Altar Interaction");
        gameObject.layer = 12;

       StartCoroutine(AltarDialogRunner.DialogCoroutine(dialogServices, startingNode));
    }

    public void EndInteracting(GameObject interactor)
    {
        Debug.Log("End Altar Interaction");
        gameObject.layer = 0;
        dialogServices.Aborted = true;
        
    }

    public void SubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd -= action;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerInteractionHandler>(out var pi))
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
