using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPricesParser
{
    const string PATH = "ShopPricesData";

    private Dictionary<ItemType, ItemAmountPair> itemPricingTable;
    private PickaxeUpgrade[] upgradePricingTable;

    public ShopPricesParser()
    {
        LoadItemPricesFromExcelTable();
        LoadUpgradePricesScriptableObjects();
    }

    private void LoadUpgradePricesScriptableObjects()
    {
        upgradePricingTable = Resources.LoadAll<PickaxeUpgrade>("PickaxeUpgrades");

        if (upgradePricingTable == null || upgradePricingTable.Length <= 0)
            Debug.Log("No Pickaxe Upgrades were loaded");
    }

    private void LoadItemPricesFromExcelTable()
    {
        DurationTracker tracker = new DurationTracker("ShopPricesParser");

        if (CSVHelper.ResourceMissing(PATH))
            return;

        itemPricingTable = new Dictionary<ItemType, ItemAmountPair>();
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

            if (!itemPricingTable.ContainsKey(product))
                itemPricingTable.Add(product, new ItemAmountPair(costType, amount));
        }

        tracker.Stop();
    }

    /// <summary>
    /// Returns -1 if no price is given
    /// </summary>
    public ItemAmountPair GetPriceFor(ItemType type, int count)
    {
        if (itemPricingTable.ContainsKey(type))
        {
            var value = itemPricingTable[type];

            return new ItemAmountPair(value.type, value.amount * count);
        }

        return new ItemAmountPair(ItemType.None, 69);
    }

    public ItemAmountPair GetPriceFor(UpgradeType type, int currentLevel)
    {
        foreach (PickaxeUpgrade upgrade in upgradePricingTable)
        {
            if (type == upgrade.Type && upgrade.RequiredLevel == currentLevel)
                return upgrade.Costs;
        }

        Debug.LogWarning("No prices for an upgrade from pickaxe level " + currentLevel + " found.");
        return new ItemAmountPair(ItemType.Diamond,99999999);
    }
}