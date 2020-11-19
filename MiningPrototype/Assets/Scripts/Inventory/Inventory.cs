using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class Inventory
{
    [SerializeField] List<ItemAmountPair> content = new List<ItemAmountPair>();

    [field: NonSerialized]
    public event System.Action InventoryChanged;

    public ItemAmountPair this[int index]
    {
        get
        {
            if (index < 0 || index >= content.Count)
                return ItemAmountPair.Nothing;

            return content[index];
        }
    }

    public void Add(ItemType type, int amount)
    {
        if (content.Count > 0)
        {
            for (int i = 0; i < content.Count; i++)
            {
                var item = content[i];

                if (item.type == type)
                {
                    content[i] = new ItemAmountPair(item.type, item.amount + amount);
                    return;
                }
            }
        }

        content.Add(new ItemAmountPair(type, amount));
        InventoryChanged?.Invoke();
    }

    public void Add(ItemAmountPair pair)
    {
        Add(pair.type, pair.amount);
    }

    public bool Contains(ItemAmountPair pair)
    {
        int id = GetStackIdFor(pair.type);

        if (id >= 0)
        {
            if (content[id].amount >= pair.amount)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryRemove(ItemAmountPair pair)
    {
        int id = GetStackIdFor(pair.type);

        if (id >= 0)
        {
            if (content[id].amount > pair.amount)
            {
                var newPair = new ItemAmountPair(pair.type, content[id].amount - pair.amount);
                content[id] = newPair;

                InventoryChanged?.Invoke();
                return true;
            }
            else if (content[id].amount == pair.amount)
            {
                content.RemoveAt(id);

                InventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    private int GetStackIdFor(ItemType type)
    {
        return content.FindIndex((x) => x.type == type);
    }

    public ItemAmountPair RemoveStack(int index)
    {
        if (index < 0 || index >= content.Count)
            return ItemAmountPair.Nothing;

        var c = content[index];

        if (c.IsNull())
        {
            content.RemoveAt(index);
            InventoryChanged?.Invoke();
            return c;
        }

        return ItemAmountPair.Nothing;
    }

    public int GetTotalWeight()
    {
        return content.Sum((x) => x.GetTotalWeight());
    }


    public KeyValuePair<ItemType, int>[] GetContent()
    {

        List<KeyValuePair<ItemType, int>> list = new List<KeyValuePair<ItemType, int>>();

        foreach (ItemAmountPair item in content)
        {
            list.Add(new KeyValuePair<ItemType, int>(item.type, item.amount));
        }

        return list.ToArray();
    }
}

[System.Serializable]
public struct ItemAmountPair
{
    public ItemType type;
    public int amount;

    public ItemAmountPair(ItemType itemType, int itemAmount)
    {
        type = itemType;
        amount = itemAmount;
    }

    public bool IsNull()
    {
        return amount <= 0 || type == ItemType.None;
    }

    public static ItemAmountPair Nothing
    {
        get => new ItemAmountPair(ItemType.None, -1);
    }

    public int GetTotalWeight()
    {
        return amount * ItemsData.GetItemInfo(type).Weight;
    }

}
