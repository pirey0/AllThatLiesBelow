using NaughtyAttributes;
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

        //add all stored items to player inventory
        if (storedItem != null && storedItem.amount > 0)
        {
            InventoryManager.PlayerCollects(storedItem.type,storedItem.amount);
            storedItem = null;
        }
    }

    [Button]
    public void Close()
    {
        SetBoxstatus(PostboxStatus.CLOSED);
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
}
