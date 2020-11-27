using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="TileMap Settings")]
public class MapSettings : ScriptableObject
{
    public int MirroringAmount;
    public TileBase[] DamageOverlayTiles;
    public TileBase[] VisibilityOverlayTiles;
    public GameObject CrumbleEffects;

}
