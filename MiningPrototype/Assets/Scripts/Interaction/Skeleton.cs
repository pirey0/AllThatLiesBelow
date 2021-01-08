using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : InventoryOwner, INonPersistantSavable
{
    public SaveID GetSavaDataID()
    {
        return new SaveID(SpawnableIDType.Skeleton);
    }

    public void Load(SpawnableSaveData data)
    {
        if(data is SkeletonSaveData ssd)
        {
            SetInventory(ssd.Inventory);
        }
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new SkeletonSaveData();
        data.SaveTransform(transform);
        data.Inventory = Inventory;

        return data;
    }

    [System.Serializable]
    public class SkeletonSaveData : SpawnableSaveData
    {
        public Inventory Inventory;
    }
}
