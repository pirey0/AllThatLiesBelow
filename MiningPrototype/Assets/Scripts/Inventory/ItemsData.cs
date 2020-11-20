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
    Support,
    Family_Letter,
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

    public static ItemInfo GetItemInfo(ItemType itemType)
    {
        if (itemInfos.ContainsKey(itemType))
        {
            return itemInfos[itemType];
        }
        return null;
    }

    public static Sprite GetSpriteByItemType(ItemAmountPair pair)
    {
        ItemInfo info = itemInfos[pair.type];
        return (info.IsReadableItem)?ReadableItemHandler.Instance?.GetSpriteOfLetter(pair.amount):info.DisplaySprite;
    }
}

