using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LetterboxStatus
{
    Closed,
    Open
}

public class Letterbox : InventoryOwner, INonPersistantSavable, IDropReceiver
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation closedEmpty, closedFull, open;
    [SerializeField] SpriteAnimation[] overflow;
    [SerializeField] AudioSource openCloseAudio, storeItemAudio;

    [Zenject.Inject] InventoryManager inventoryManager;
    LetterboxStatus status;

    private event System.Action ForceInterrupt;

    protected override void OnRealStart()
    {
        base.OnRealStart();
        SetLetterboxStatus(status);
    }

    [Button]
    public void Open()
    {
        SetLetterboxStatus(LetterboxStatus.Open);
    }

    [Button]
    public void Close()
    {
        ForceInterrupt?.Invoke();

        if (status != LetterboxStatus.Open)
            return;

       SetLetterboxStatus(LetterboxStatus.Closed);
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
            case LetterboxStatus.Closed:
                spriteAnimator.Play(GetAnimationFromAmount(Inventory.Count));
                break;

            case LetterboxStatus.Open:
                spriteAnimator.Play(open);
                break;
        }

        openCloseAudio.Play();

        status = newStatus;
    }

    private SpriteAnimation GetAnimationFromAmount(int count)
    {
        if (count == 0)
            return closedEmpty;
        else if (count > 1 && overflow != null && overflow.Length > 0)
        {
            return overflow[Mathf.Min(count - 2, overflow.Length - 1)];
        }
        else
            return closedFull;
    }

    public void AddStoredItem(ItemAmountPair itemAmountPair)
    {
        Inventory.Add(itemAmountPair);

        SetLetterboxStatus(LetterboxStatus.Closed);
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

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
         return false;
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        //
    }

    public void EndHover()
    {
        //
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair, Inventory origin)
    {
        //
    }

    [System.Serializable]
    public class LetterBoxSaveData : SpawnableSaveData
    {
        public LetterboxStatus Status;
        public Inventory Inventory;
    }
}