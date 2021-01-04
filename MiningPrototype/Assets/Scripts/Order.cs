using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public Dictionary<ItemType, int> Costs;
    public ItemAmountPair[] Items;
    public UpgradeType[] Upgrades;

    public Order (Dictionary<ItemType, int> costs, ItemAmountPair[] items, UpgradeType[] upgrades)
    {
        Costs = costs;
        Items = items;
        Upgrades = upgrades;
    }
}
