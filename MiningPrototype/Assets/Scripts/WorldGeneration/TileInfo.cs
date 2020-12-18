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
    public bool damagedByExplosion;

    public bool AirLike = false;
    public bool Targetable = true;
    public bool TargetPriority = false;

    [Space(10)]
    public CrumbleType CrumbleType = CrumbleType.Air;
    public bool CanBeUnstable = true;
    public bool NotifiesInsteadOfCrumbling = false;
    public float UnstableTimeBeforeCrumble = 5;

    [Space(10)]
    public bool CountsAsNeighbour = true;
    public bool MinableInOverworld = false;
    public bool DamageIsInBackground;
    public bool DrawsToShifted;

    [Header("Visuals")]
    public bool UseTilesFromOtherInfo;
    public TileBase[] Tiles;
    public TileInfo TileSourceInfo;

    public TileBase Visibility2Tile;
    public TileBase Overlay;


    public Sprite physicalTileSprite;
    public Sprite physicalTileOverlay;

}
