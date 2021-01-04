using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PickaxeUpgrade : ScriptableObject
{
    public UpgradeType Type;
    public int RequiredLevel;
    public int Level;
    public ItemAmountPair Costs;
    public float MiningSpeed;
}

public enum UpgradeType
{
    Pickaxe
}
