using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    Rocks,
    Gold,
    Copper,
    Family_Photo,
    Diamond,
    Ladder,
    Support
}


public static class ItemsData 
{
    static Dictionary<ItemType, ItemInfo> itemInfos;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void FindItemInfos()
    {
        itemInfos = new Dictionary<ItemType, ItemInfo>();

        ItemInfo[] items =  Resources.LoadAll<ItemInfo>("Items");

        Debug.Log("Loaded " + items.Length + " iteminfos");

        foreach (var item in items)
        {
            itemInfos.Add(item.ItemType, item);
        }
    }


    public static Sprite GetSpriteByItemType(ItemType itemType)
    {
        return itemInfos[itemType].DisplaySprite;
    }
}

