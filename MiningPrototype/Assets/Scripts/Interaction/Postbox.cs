using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PostboxStatus
{
    CLOSED,
    OPEN,
    ACTIVE,
    ACTIVEPLAYER
}

public class Postbox : MonoBehaviour, IInteractable, IDropReceiver
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation closed, open, active, activePlayer, openFull;

    [SerializeField] GameObject postboxIsFull;

    [SerializeField] AudioSource openCloseAudio, storeItemAudio;

    [SerializeField] ItemAmountPair storedItem;
    [SerializeField] string storedMessage;

    [SerializeField] Canvas canvas;
    [SerializeField] ReadableItemVisualizer messagePrefab;
    ReadableItemVisualizer displayedMessage;

    [SerializeField] Canvas orderCanvas;
    [SerializeField] NewOrderVisualizer newOrderPrefab;
    NewOrderVisualizer newOrder;

    PostboxStatus status;

    [Button]
    public void Activate()
    {
        SetBoxstatus(PostboxStatus.ACTIVE);
    }

    [Button]
    public void Open()
    {
        //add all stored items to player inventory
        if (!storedItem.IsNull() && storedItem.amount > 0)
        {
            InventoryManager.PlayerCollects(storedItem.type, storedItem.amount);
            storedItem = ItemAmountPair.Nothing;
        }
        SetBoxstatus(PostboxStatus.OPEN);
    }

    [Button]
    public void Close()
    {
        if (status != PostboxStatus.OPEN)
            return;

        if (IsEmpty())
            SetBoxstatus(PostboxStatus.CLOSED);
        else
            SetBoxstatus(PostboxStatus.ACTIVEPLAYER);
    }

    public void BeginInteracting(GameObject interactor)
    {
        Open();
    }

    public void EndInteracting(GameObject interactor)
    {
        Debug.Log("Postbox end interacting.");
        Close();
    }

    private void SetBoxstatus(PostboxStatus newStatus)
    {
        if (newStatus != status)
        {
            switch (newStatus)
            {
                case PostboxStatus.ACTIVE:
                    spriteAnimator.Play(active);
                    break;

                case PostboxStatus.ACTIVEPLAYER:
                    spriteAnimator.Play(activePlayer);
                    break;

                case PostboxStatus.OPEN:
                    spriteAnimator.Play(IsEmpty() ? open : openFull);
                    break;

                case PostboxStatus.CLOSED:
                    spriteAnimator.Play(closed);
                    break;
            }

            openCloseAudio.Play();

            status = newStatus;
        }
    }

    public ItemAmountPair GetStoredItem()
    {
        return storedItem;
    }

    public void SetStoredItem(ItemAmountPair itemAmountPair)
    {
        storedItem = itemAmountPair;
        if (IsEmpty())
            SetBoxstatus(PostboxStatus.CLOSED);
        else
            SetBoxstatus(PostboxStatus.ACTIVE);
    }

    public void SubscribeToForceQuit(Action action)
    {
    }

    public void UnsubscribeToForceQuit(Action action)
    {
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return (storedItem.IsNull());
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        SetBoxstatus(PostboxStatus.OPEN);
        postboxIsFull.SetActive(!IsEmpty());
    }

    public void EndHover()
    {
        if (IsEmpty())
            SetBoxstatus(PostboxStatus.CLOSED);

        postboxIsFull.SetActive(false);
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair)
    {
        if (InventoryManager.PlayerTryPay(pair.type, pair.amount))
        {
            storedItem = pair;
            storeItemAudio?.Play();
            SetBoxstatus(PostboxStatus.ACTIVEPLAYER);
        }
    }

    public bool IsEmpty()
    {
        return storedItem.IsNull();
    }
}
