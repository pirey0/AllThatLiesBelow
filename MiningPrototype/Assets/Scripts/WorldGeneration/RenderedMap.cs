using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RenderedMap : BaseMap
{
    [Header("RenderedMap")]
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap damageOverlayTilemap, oreTilemap;

    protected virtual void Setup()
    {

        tilemap.GetComponent<ITileMapElement>()?.Setup(this);
        damageOverlayTilemap.GetComponent<ITileMapElement>()?.Setup(this);
        oreTilemap.GetComponent<ITileMapElement>()?.Setup(this);
    }
    
    protected override void UpdateAllVisuals()
    {
        tilemap.ClearAllTiles();
        damageOverlayTilemap.ClearAllTiles();
        oreTilemap.ClearAllTiles();
        Util.IterateXY(SizeX, SizeY, UpdateVisualsAt);
    }

    protected override void UpdateVisualsAt(int x, int y)
    {
        SetVisualsAt(x, y);
        foreach (var nIndex in MapHelper.GetNeighboursIndiciesOf(x, y))
        {
            SetVisualsAt(nIndex.x, nIndex.y);
        }
    }

    private void SetVisualsAt(int x, int y)
    {
        WrapXIfNecessary(ref x);

        if (IsOutOfBounds(x, y))
        {
            return;
        }

        var tile = GetVisualTileFor(x, y);
        var destTile = GetVisualDestructableOverlayFor(x, y);
        var oreTile = GetVisualOverlayTileFor(x, y);

        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), destTile);
        oreTilemap.SetTile(new Vector3Int(x, y, 0), oreTile);

        if (x < Settings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(SizeX + x, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), oreTile);
        }

        if (x > SizeX - Settings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(x - SizeX, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), oreTile);
        }
    }

    private TileBase GetVisualTileFor(int x, int y)
    {
        Tile tile = map[x, y];

        if (IsOutOfBounds(x, y) || IsAirAt(x, y))
            return null;


        int tileIndex = Util.BITMASK_TO_TILEINDEX[tile.NeighbourBitmask];

        TileInfo tileInfo = GetTileInfo(tile.Type);
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
        return Settings.DamageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    private TileBase GetVisualOverlayTileFor(int x, int y)
    {
        var t = map[x, y];
        var info = GetTileInfo(t.Type);
        return info.Overlay;
    }
}
