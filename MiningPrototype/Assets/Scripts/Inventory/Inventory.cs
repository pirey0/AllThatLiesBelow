using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Inventory
{
    [SerializeField] List<ItemAmountPair> content = new List<ItemAmountPair>();

    public event System.Action InventoryChanged;

    public void Add(itemType type, int amount)
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

    public ItemAmountPair Remove(int index)
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


    public KeyValuePair<itemType, int>[] GetContent()
    {

        List<KeyValuePair<itemType, int>> list = new List<KeyValuePair<itemType, int>>();

        foreach (ItemAmountPair item in content)
        {
            list.Add(new KeyValuePair<itemType, int>(item.type, item.amount));
        }

        return list.ToArray();
    }
}

[System.Serializable]
public class ItemAmountPair
{
    public ItemAmountPair(itemType itemType, int itemAmount)
    {
        type = itemType;
        amount = itemAmount;
    }

    public itemType type;
    public int amount;
}
