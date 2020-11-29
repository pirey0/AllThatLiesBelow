using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DropBox : HoverHighlighter, IDropReceiver, INonPersistantSavable
{
    [SerializeField] ItemType[] sendable;
    [SerializeField] AudioSource storeItemAudio;

    [SerializeField] Inventory inventory = new Inventory();
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
        if (WouldTakeDrop(pair) && origin.Contains(pair) && origin.TryRemove(pair))
        {
            inventory.Add(pair);
            storeItemAudio?.Play();
        }
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return sendable.Contains(pair.type);
    }

    public ItemAmountPair FetchItem()
    {
        return inventory.Pop();
    }

    public bool IsEmpty()
    {
        return inventory.IsEmpty();
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new DropBoxSaveData();
        data.SpawnableIDType = SpawnableIDType.DropBox;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.Inventory = inventory;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if(dataOr is DropBoxSaveData data)
        {
            inventory = data.Inventory;
        }
    }

    [System.Serializable]
    public class DropBoxSaveData : SpawnableSaveData
    {
        public Inventory Inventory;
    }
}
