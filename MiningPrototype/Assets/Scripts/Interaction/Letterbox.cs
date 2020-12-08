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

public class LetterBox : InventoryOwner, INonPersistantSavable
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation closedEmpty, closedFull, open;
    [SerializeField] AudioSource openCloseAudio, storeItemAudio;

    [Zenject.Inject] InventoryManager inventoryManager;
    LetterboxStatus status;

    private event System.Action ForceInterrupt;

    protected override void OnStartAfterLoad()
    {
        base.OnStartAfterLoad();
        SetLetterboxStatus(status);
    }

    [Button]
    public void Open()
    {
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

    public override void BeginInteracting(GameObject interactor)
    {
        base.BeginInteracting(interactor);
        Open();
    }

    public override void EndInteracting(GameObject interactor)
    {
        base.EndInteracting(interactor);
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
        Inventory.Add(itemAmountPair);

        if (IsEmpty())
            SetLetterboxStatus(LetterboxStatus.ClosedEmpty);
        else
            SetLetterboxStatus(LetterboxStatus.ClosedFull);
    }

  
    public bool IsEmpty()
    {
        return Inventory.IsEmpty();
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new LetterBoxSaveData();
        data.SpawnableIDType = SpawnableIDType.LetterBox;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.Inventory = Inventory;
        data.Status = status;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is LetterBoxSaveData data)
        {
            SetInventory(data.Inventory);
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