using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    [SerializeField] Map map;
    [SerializeField] Tilemap tilemap, damageOverlayTilemap, oreTilemap;

    private Map Map { get => map; }
    private int SizeX { get => Map.SizeX; }
    private int SizeY { get => Map.SizeY; }

    private void Start()
    {
        if (map == null)
        {
            Destroy(this);
            return;
        }

        Setup();
        UpdateVisuals();
    }

    [Button]
    public void Setup()
    {
        if (map == null)
            return;

        tilemap.GetComponent<ITileMapElement>()?.Setup(Map);
        damageOverlayTilemap.GetComponent<ITileMapElement>()?.Setup(Map);
        oreTilemap.GetComponent<ITileMapElement>()?.Setup(Map);

        map.FullVisualUpdate += UpdateVisuals;
        map.VisualUpdateAt += UpdateVisualsAt;
    }

    [Button]
    void UpdateVisuals()
    {
        tilemap.ClearAllTiles();
        damageOverlayTilemap.ClearAllTiles();
        oreTilemap.ClearAllTiles();
        Util.IterateXY(Map.SizeX, Map.SizeY, UpdateVisualsAt);
    }


    private void UpdateVisualsAt(int x, int y)
    {
        Map.WrapXIfNecessary(ref x);

        if (Map.IsOutOfBounds(x, y))
        {
            return;
        }

        var tile = GetVisualTileFor(x, y);
        var destTile = GetVisualDestructableOverlayFor(x, y);
        var oreTile = GetVisualOverlayTileFor(x, y);

        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), destTile);
        oreTilemap.SetTile(new Vector3Int(x, y, 0), oreTile);

        if (x < Map.Settings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(SizeX + x, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), oreTile);
        }

        if (x > SizeX - Map.Settings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(x - SizeX, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), oreTile);
        }
    }

    private TileBase GetVisualTileFor(int x, int y)
    {
        Tile tile = Map[x, y];

        if (Map.IsOutOfBounds(x, y) || Map.IsAirAt(x, y))
            return null;


        int tileIndex = Util.BITMASK_TO_TILEINDEX[tile.NeighbourBitmask];

        TileInfo tileInfo = Map.GetTileInfo(tile.Type);
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
        var t = Map[x, y];
        return Map.Settings.DamageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    private TileBase GetVisualOverlayTileFor(int x, int y)
    {
        var t = Map[x, y];
        var info = Map.GetTileInfo(t.Type);
        return info.Overlay;
    }
}
