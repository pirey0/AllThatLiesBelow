using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PickaxeUpgrade : ScriptableObject
{
    public ItemType Type;
    public int RequiredLevel;
    public int Level;
    public string DisplayName;
    public ItemAmountPair Costs;
    public float MiningSpeed;
}

public enum UpgradeType
{
    Pickaxe,
}
