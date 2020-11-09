using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Inventory 
{
    [SerializeField] List<ItemAmountPair>  content = new List<ItemAmountPair>();

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
    } 

    public KeyValuePair<itemType, int>[] GetContent()
    {

        List<KeyValuePair<itemType, int>> list = new List<KeyValuePair<itemType, int>>();

        foreach (ItemAmountPair item in content)
        {
            list.Add(new KeyValuePair<itemType, int>(item.type,item.amount));
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
