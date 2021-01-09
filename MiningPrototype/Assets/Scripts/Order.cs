using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILetter { }

[System.Serializable]
public class Order : ILetter
{
    public Dictionary<ItemType, int> Costs;
    public ItemAmountPair[] Items;
    public UpgradeType[] Upgrades;

    public Order(Dictionary<ItemType, int> costs, ItemAmountPair[] items, UpgradeType[] upgrades)
    {
        Costs = costs;
        Items = items;
        Upgrades = upgrades;
    }
}

[System.Serializable]
public class LetterToFamily : ILetter
{
    public LetterType Type;

    public LetterToFamily(LetterType t)
    {
        Type = t;
    }

    public enum LetterType
    {
        Payed10,
        Payed100,
        Payed1000
    }
}