using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName ="TileMap Settings")]
public class TileMapSettings : ScriptableObject
{
    public TileBase[] DamageOverlayTiles;

    public GameObject CrumbleEffects;
}
