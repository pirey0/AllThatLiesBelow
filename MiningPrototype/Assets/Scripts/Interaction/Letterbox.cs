using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LetterboxStatus
{
    CLOSED,
    OPEN,
    ACTIVE,
    ACTIVEPLAYER
}

public class Letterbox : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation closed, open, active, activePlayer, openFull;

    [SerializeField] GameObject letterboxIsFull;

    [SerializeField] AudioSource openCloseAudio, storeItemAudio;

    [SerializeField] ItemAmountPair storedItem;
    [SerializeField] string storedMessage;

    [SerializeField] Canvas canvas;
    [SerializeField] ReadableItemVisualizer messagePrefab;
    ReadableItemVisualizer displayedMessage;

    [SerializeField] Canvas orderCanvas;
    [SerializeField] NewOrderVisualizer newOrderPrefab;
    NewOrderVisualizer newOrder;

    [Zenject.Inject] InventoryManager inventoryManager;

    private event System.Action ForceInterrupt;

    LetterboxStatus status;

    [Button]
    public void Activate()
    {
        SetLetterboxStatus(LetterboxStatus.ACTIVE);
    }

    [Button]
    public void Open()
    {
        //add all stored items to player inventory
        if (!storedItem.IsNull() && storedItem.amount > 0)
        {
            inventoryManager.PlayerCollects(storedItem.type, storedItem.amount);
            storedItem = ItemAmountPair.Nothing;
        }
        SetLetterboxStatus(LetterboxStatus.OPEN);
    }

    [Button]
    public void Close()
    {
        ForceInterrupt?.Invoke();

        if (status != LetterboxStatus.OPEN)
            return;

        if (IsEmpty())
            SetLetterboxStatus(LetterboxStatus.CLOSED);
        else
            SetLetterboxStatus(LetterboxStatus.ACTIVEPLAYER);
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

    private void SetLetterboxStatus(LetterboxStatus newStatus)
    {
        if (newStatus != status)
        {
            switch (newStatus)
            {
                case LetterboxStatus.ACTIVE:
                    spriteAnimator.Play(active);
                    break;

                case LetterboxStatus.ACTIVEPLAYER:
                    spriteAnimator.Play(activePlayer);
                    break;

                case LetterboxStatus.OPEN:
                    spriteAnimator.Play(IsEmpty() ? open : openFull);
                    break;

                case LetterboxStatus.CLOSED:
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
            SetLetterboxStatus(LetterboxStatus.CLOSED);
        else
            SetLetterboxStatus(LetterboxStatus.ACTIVE);
    }

    public void SubscribeToForceQuit(Action action)
    {
        ForceInterrupt += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        ForceInterrupt -= action;
    }

    public bool IsEmpty()
    {
        return storedItem.IsNull();
    }
}
