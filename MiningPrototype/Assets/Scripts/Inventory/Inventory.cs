using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Inventory
{
    [SerializeField] List<ItemAmountPair> content = new List<ItemAmountPair>();

    [field: NonSerialized]
    public event System.Action InventoryChanged;

    public void Add(ItemType type, int amount)
    {
        if (content.Count > 0)
        {
            foreach (ItemAmountPair item in content)
            {
                if (item.type == type)
                {
                    item.amount += amount;
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
            if(content[id].amount > pair.amount)
            {
                content[id].amount -= pair.amount;

                InventoryChanged?.Invoke();
                return true;
            }else if (content[id].amount == pair.amount)
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
            return null;

        var c = content[index];

        if(c != null)
        {
            content.RemoveAt(index);
            InventoryChanged?.Invoke();
            return c;
        }

        return null;
    }

    public ItemAmountPair this[int index]
    {
        get
        {
            if (index < 0 || index >= content.Count)
                return null;

            return content[index];
        }
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
public class ItemAmountPair
{
    public ItemAmountPair(ItemType itemType, int itemAmount)
    {
        type = itemType;
        amount = itemAmount;
    }

    public ItemType type;
    public int amount;
}
