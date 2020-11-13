using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="TileMap Settings")]
public class TileMapSettings : ScriptableObject
{

    public TileBase[] GroundTiles;
    public TileBase[] SnowTiles;
    public TileBase[] DamageOverlayTiles;
    public TileBase[] OreTiles;
}
