using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LetterboxStatus
{
    ClosedEmpty,
    ClosedFull,
    OPEN
}

public class LetterBox : StateListenerBehaviour, IInteractable, INonPersistantSavable
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation closedEmpty, closedFull, open;
    [SerializeField] AudioSource openCloseAudio, storeItemAudio;

    [Zenject.Inject] InventoryManager inventoryManager;

    Inventory inventory = new Inventory();
    LetterboxStatus status;

    private event System.Action ForceInterrupt;

    protected override void OnStartAfterLoad()
    {
        SetLetterboxStatus(status);
    }

    [Button]
    public void Open()
    {
        //add all stored items to player inventory
        var element = inventory.Pop();
        while (element != ItemAmountPair.Nothing)
        {
            inventoryManager.PlayerCollects(element.type, element.amount);
            element = inventory.Pop();
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
            SetLetterboxStatus(LetterboxStatus.ClosedEmpty);
        else
            SetLetterboxStatus(LetterboxStatus.ClosedFull);
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
        Debug.Log("Switching to: " + newStatus);
        switch (newStatus)
        {
            case LetterboxStatus.ClosedFull:
                spriteAnimator.Play(closedFull);
                break;

            case LetterboxStatus.OPEN:
                spriteAnimator.Play(open);
                break;

            case LetterboxStatus.ClosedEmpty:
                spriteAnimator.Play(closedEmpty);
                break;
        }

        openCloseAudio.Play();

        status = newStatus;
    }


    public void AddStoredItem(ItemAmountPair itemAmountPair)
    {
        inventory.Add(itemAmountPair);

        if (IsEmpty())
            SetLetterboxStatus(LetterboxStatus.ClosedEmpty);
        else
            SetLetterboxStatus(LetterboxStatus.ClosedFull);
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
        return inventory.IsEmpty();
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new LetterBoxSaveData();
        data.SpawnableIDType = SpawnableIDType.LetterBox;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.Inventory = inventory;
        data.Status = status;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is LetterBoxSaveData data)
        {
            inventory = data.Inventory;
            status = data.Status;
        }
    }

    [System.Serializable]
    public class LetterBoxSaveData : SpawnableSaveData
    {
        public LetterboxStatus Status;
        public Inventory Inventory;
    }
}