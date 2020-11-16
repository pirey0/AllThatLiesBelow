using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "TileInfo")]
public class TileInfo : ScriptableObject
{
    public TileType Type = TileType.Air;
    public ItemType ItemToDrop = ItemType.None;
    public float damageMultiplyer = 1;

    public bool AirLike = false;
    public bool Targetable = true;
    public bool StabilityAffected = true;

    public TileBase[] Tiles;
    public bool UseTilesFromOtherInfo;
    public TileInfo TileSourceInfo;

    public TileBase Overlay;
    
}
