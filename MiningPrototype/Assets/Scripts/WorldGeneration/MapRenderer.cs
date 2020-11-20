using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    [SerializeField] bool on;
    [SerializeField] Map map;
    [SerializeField] Tilemap tilemap, damageOverlayTilemap, oreTilemap;

    private int SizeX { get => map.SizeX; }
    private int SizeY { get => map.SizeY; }

    public void Setup(Map map)
    {
        if (map == null)
            return;

        this.map = map;

        tilemap.GetComponent<ITileMapElement>()?.Setup(map);
        damageOverlayTilemap.GetComponent<ITileMapElement>()?.Setup(map);
        oreTilemap.GetComponent<ITileMapElement>()?.Setup(map);
    }

    [Button]
    public void UpdateVisuals()
    {
        if (!on)
            return;

        tilemap.ClearAllTiles();
        damageOverlayTilemap.ClearAllTiles();
        oreTilemap.ClearAllTiles();
        Util.IterateXY(map.SizeX, map.SizeY, UpdateVisualsAt);
    }


    public void UpdateVisualsAt(int x, int y)
    {
        if (!on)
            return;

        map.WrapXIfNecessary(ref x);

        if (map.IsOutOfBounds(x, y))
        {
            return;
        }

        var tile = GetVisualTileFor(x, y);
        var destTile = GetVisualDestructableOverlayFor(x, y);
        var oreTile = GetVisualOverlayTileFor(x, y);

        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), destTile);
        oreTilemap.SetTile(new Vector3Int(x, y, 0), oreTile);

        if (x < map.Settings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(SizeX + x, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), oreTile);
        }

        if (x > SizeX - map.Settings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(x - SizeX, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), oreTile);
        }
    }

    private TileBase GetVisualTileFor(int x, int y)
    {
        Tile tile = map[x, y];

        if (map.IsOutOfBounds(x, y) || map.IsAirAt(x, y))
            return null;


        int tileIndex = Util.BITMASK_TO_TILEINDEX[tile.NeighbourBitmask];

        TileInfo tileInfo = map.GetTileInfo(tile.Type);
        TileBase tileVis = null;

        if (tileInfo.UseTilesFromOtherInfo && tileInfo.TileSourceInfo != null)
        {
            tileInfo = tileInfo.TileSourceInfo;
        }

        if (tileInfo.Tiles.Length >= 48)
        {
            tileVis = tileInfo.Tiles[tileIndex];
        }

        return tileVis;
    }

    private TileBase GetVisualDestructableOverlayFor(int x, int y)
    {
        var t = map[x, y];
        return map.Settings.DamageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    private TileBase GetVisualOverlayTileFor(int x, int y)
    {
        var t = map[x, y];
        var info = map.GetTileInfo(t.Type);
        return info.Overlay;
    }
}
