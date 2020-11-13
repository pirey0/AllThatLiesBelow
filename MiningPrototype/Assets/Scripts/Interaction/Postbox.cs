using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PostboxStatus
{
    CLOSED,
    OPEN,
    ACTIVE
}

public class Postbox : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite closed, open, active;

    [SerializeField] AudioSource audioSource;

    [SerializeField] ItemAmountPair storedItem;
    [SerializeField] string storedMessage;

    [SerializeField] Canvas canvas;
    [SerializeField] InteractableMessageVisualizer messagePrefab;
    InteractableMessageVisualizer displayedMessage;

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
        SetBoxstatus(PostboxStatus.OPEN);

        //display message
        if (messagePrefab != null && displayedMessage == null && storedMessage != "")
        {
            displayedMessage = Instantiate(messagePrefab, canvas.transform);

            displayedMessage.DisplayText(transform, storedMessage, showFamilyPhoto: (storedItem != null && storedItem.type == ItemType.Family_Photo));
            storedMessage = "";

            //add all stored items to player inventory
            if (storedItem != null && storedItem.amount > 0)
            {
                InventoryManager.PlayerCollects(storedItem.type,storedItem.amount);
                storedItem = null;
            }
        }

        //show order formular
        else
        {
            if (newOrder == null)
                newOrder = Instantiate(newOrderPrefab,orderCanvas.transform);
        }
    }

    [Button]
    public void Close()
    {
        SetBoxstatus(PostboxStatus.CLOSED);

        //hide message if one is displayed
        if (displayedMessage != null)
            displayedMessage.Hide();

        if (newOrder != null)
            newOrder.Cancel();
    }

    public void BeginInteracting(GameObject interactor)
    {
        Open();
    }

    public void EndInteracting(GameObject interactor)
    {
        Close();
    }

    private void SetBoxstatus(PostboxStatus newStatus)
    {
        if (newStatus != status)
        {
            switch (newStatus)
            {
                case PostboxStatus.ACTIVE:
                    spriteRenderer.sprite = active;
                    break;

                case PostboxStatus.OPEN:
                    spriteRenderer.sprite = open;
                    break;

                case PostboxStatus.CLOSED:
                    spriteRenderer.sprite = closed;
                    break;
            }

            if (newStatus != PostboxStatus.ACTIVE)
                audioSource.Play();

            status = newStatus;
        }
    }

    public void SubscribeToForceQuit(Action action)
    {
    }

    public void UnsubscribeToForceQuit(Action action)
    {
    }
}
