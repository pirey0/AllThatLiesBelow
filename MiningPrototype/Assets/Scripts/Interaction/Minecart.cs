using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minecart : MirrorWorldFollower, INonPersistantSavable
{
    [SerializeField] private InventoryOwner inventoryOwner;
    public SaveID GetSavaDataID()
    {
        return new SaveID("Minecart");
    }
    public SpawnableSaveData ToSaveData()
    {
        var data = new MinecartSaveData();
        data.SaveTransform(transform);
        data.Inventory = inventoryOwner.Inventory;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is MinecartSaveData data)
        {
            inventoryOwner.SetInventory(data.Inventory);
        }
    }

    [System.Serializable]
    public class MinecartSaveData : SpawnableSaveData
    {
        public Inventory Inventory;
    }
}
