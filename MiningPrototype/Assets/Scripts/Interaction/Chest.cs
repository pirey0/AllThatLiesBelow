using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : FallingTilemapCarvingEntity, IDropReceiver, INonPersistantSavable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite chestOpen, chestClosed;
    [SerializeField] InventoryOwner inventoryOwner;

    protected override void Start()
    {
        //Force chests in correct location to fix placement errors in scenes
        if (transform.position.x % 0.5f != 0)
        {
            transform.position = new Vector3(transform.position.x + 0.5f - (transform.position.x % 0.5f), transform.position.y);
        }

        base.Start();
        inventoryOwner.StateChanged += OnStateChanged;
        Carve();
    }

    private void OnStateChanged(InventoryState obj)
    {
        switch (obj)
        {
            case InventoryState.Open:
                spriteRenderer.sprite = chestOpen;
                break;

            case InventoryState.Closed:
                spriteRenderer.sprite = chestClosed;
                break;
        }
    }

    public override void UncarveDestroy()
    {
        inventoryManager.PlayerCollects(inventoryOwner.Inventory.GetContent());
        base.UncarveDestroy();
    }

    public void HoverUpdate()
    {
        //
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return true;
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
        if (origin.Contains(pair) && origin.TryRemove(pair))
            inventoryOwner.Inventory.Add(pair);
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new ChestSaveData();
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.Inventory = inventoryOwner.Inventory;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is ChestSaveData data)
        {
            inventoryOwner.SetInventory(data.Inventory);
        }
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (this == null || reason != TileUpdateReason.Destroy)
            return;

        UncarveDestroy();
        Debug.Log("Destroying chest " + reason);
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID(SpawnableIDType.Chest);
    }

    public bool IsSameInventory(Inventory inventory)
    {
        return inventoryOwner.Inventory == inventory;
    }

    [System.Serializable]
    public class ChestSaveData : SpawnableSaveData
    {
        public Inventory Inventory;
    }
}
