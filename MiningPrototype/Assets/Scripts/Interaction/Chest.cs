using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InventoryOwner, IHoverable, IDropReceiver, INonPersistantSavable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite chestOpen, chestClosed;
    [SerializeField] Material defaultMat, outlineMat;

    public override void OpenInventory()
    {
        base.OpenInventory();
        spriteRenderer.sprite = chestOpen;
    }

    public override void CloseInventory()
    {
        base.CloseInventory();
        spriteRenderer.sprite = chestClosed;
    }

    public void HoverEnter()
    {
        spriteRenderer.material = outlineMat;
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f);
}

    public void HoverExit()
    {
        spriteRenderer.material = defaultMat;
        spriteRenderer.color = Color.white;
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
            Inventory.Add(pair);
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new ChestSaveData();
        data.SpawnableIDType = SpawnableIDType.Chest;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.Inventory = Inventory;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if(dataOr is ChestSaveData data)
        {
            transform.position = data.Position.ToVector3();
            transform.eulerAngles = data.Rotation.ToVector3();
            SetInventory(data.Inventory);
        }
    }

    [System.Serializable]
    public class ChestSaveData : SpawnableSaveData
    {
        public Inventory Inventory;
    }
}
