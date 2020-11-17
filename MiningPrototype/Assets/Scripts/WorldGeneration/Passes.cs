using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrePass 
{
    public TileType TileType;
    public Vector2Int OreVeinSize;
    public AnimationCurve Probability;
}


[System.Serializable]
public class RockPass
{
    public Vector2Int Size;
    public AnimationCurve Probability;
    public GameObject Prefab;
}