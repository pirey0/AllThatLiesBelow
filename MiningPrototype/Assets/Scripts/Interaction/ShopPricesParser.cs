using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPricesParser
{
    const string PATH = "ShopPricesData";

    private static Dictionary<ItemType, ItemAmountPair> pricingTable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ParsePrices()
    {
        DurationTracker tracker = new DurationTracker("ShopPricesParser");

        if (CSVHelper.ResourceMissing(PATH))
            return;

        pricingTable = new Dictionary<ItemType, ItemAmountPair>();
        string[,] table = CSVHelper.LoadTableAtPath(PATH);


        for (int y = 1; y < table.GetLength(1); y++)
        {
            ItemType product = ItemType.None;
            if (Enum.TryParse<ItemType>(table[0, y], out ItemType result))
            {
                product = result;
            }
            else
            {
                Debug.Log("No ItemType match found for " + table[0, y]);
            }

            ItemType costType = ItemType.None;
            if (Enum.TryParse<ItemType>(table[2, y], out result))
            {
                costType = result;
            }
            else
            {
                Debug.Log("No ItemType match found for " + table[2, y]);
            }

            int amount = -1;
            if (int.TryParse(table[1, y], out int value))
            {
                amount = value;
            }
            else
            {
                Debug.Log("Cannot convert " + table[1, y] + " to int");
            }

            if (!pricingTable.ContainsKey(product))
                pricingTable.Add(product, new ItemAmountPair(costType, amount));
        }

        tracker.Stop();
    }

    /// <summary>
    /// Returns -1 if no price is given
    /// </summary>
    public static ItemAmountPair GetPriceFor(ItemType type, int count)
    {
        if (pricingTable.ContainsKey(type))
        {
            var value = pricingTable[type];

            return new ItemAmountPair(value.type, value.amount * count);
        }

        return new ItemAmountPair(ItemType.None, 69);
    }
}