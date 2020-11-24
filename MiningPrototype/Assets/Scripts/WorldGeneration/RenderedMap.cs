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
    
    public override void UpdateAllVisuals()
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

    protected override void UpdatePropertiesAt(int x, int y, Tile newTile, Tile previousTile, TileUpdateReason reason)
    {
        CalculateNeighboursBitmaskAt(x, y);

        foreach (var nIndex in MapHelper.GetNeighboursIndiciesOf(x, y))
        {
            CalculateNeighboursBitmaskAt(nIndex.x, nIndex.y);
        }

        PropagateStabilityUpdatesFrom(x, y);
    }

    private void CalculateNeighboursBitmaskAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return;

        int topLeft = IsNeighbourAt(x - 1, y + 1) ? 1 : 0;
        int topMid = IsNeighbourAt(x, y + 1) ? 1 : 0;
        int topRight = IsNeighbourAt(x + 1, y + 1) ? 1 : 0;
        int midLeft = IsNeighbourAt(x - 1, y) ? 1 : 0;
        int midRight = IsNeighbourAt(x + 1, y) ? 1 : 0;
        int botLeft = IsNeighbourAt(x - 1, y - 1) ? 1 : 0;
        int botMid = IsNeighbourAt(x, y - 1) ? 1 : 0;
        int botRight = IsNeighbourAt(x + 1, y - 1) ? 1 : 0;

        int value = topMid * 2 + midLeft * 8 + midRight * 16 + botMid * 64;
        value += topLeft * topMid * midLeft;
        value += topRight * topMid * midRight * 4;
        value += botLeft * midLeft * botMid * 32;
        value += botRight * midRight * botMid * 128;

        Tile t = this[x, y];
        t.NeighbourBitmask = (byte)value;
        this[x, y] = t;

    }

    private void PropagateStabilityUpdatesFrom(int x, int y)
    {
        MarkToCheckForStability(x, y);
        ResetStability(x, y);
        DirectionalStabilityIterator(x, y, Direction.Up);
        DirectionalStabilityIterator(x, y, Direction.Right);
        DirectionalStabilityIterator(x, y, Direction.Down);
        DirectionalStabilityIterator(x, y, Direction.Left);
    }

    private void DirectionalStabilityIterator(int x, int y, Direction dir)
    {
        Vector2Int offset = dir.Inverse().AsV2Int();

        for (int i = 0; i < GenerationSettings.StabilityPropagationDistance; i++)
        {
            SetDirectionalStabilityAt(x, y, dir);
            MarkToCheckForStability(x, y);
            x += offset.x;
            y += offset.y;
        }
    }

    private void ResetStability(int x, int y)
    {
        var tile = GetTileAt(x, y);
        tile.ResetStability();

        this[x, y] = tile;
    }

    private void SetDirectionalStabilityAt(int x, int y, Direction direction)
    {
        if (IsAirAt(x, y))
            return;

        var tile = GetTileAt(x, y);

        if (direction == Direction.Down)
            tile.SetStability(direction, IsBlockAt(x, y - 1) ? 100 : 0);
        else
        {
            Vector2Int offset = direction.AsV2Int();
            tile.SetStability(direction, Mathf.Min(25, this[x + offset.x, y + offset.y].Stability / 4));
        }

        this[x, y] = tile;
    }

    protected virtual void MarkToCheckForStability(int x, int y)
    {
    }

    public override void CalculateStabilityAll()
    {
        Util.IterateXY(SizeX, SizeY, (x, y) => ResetStability(x, y));

        Util.IterateXY(SizeX, SizeY, (x, y) => SetDirectionalStabilityAt(x, y, Direction.Down));

        for (int y = SizeY; y >= 0; y--)
        {
            for (int x = 0; x < SizeX; x++)
            {
                SetDirectionalStabilityAt(x, y, Direction.Up);
            }
        }

        for (int y = 0; y < SizeY; y++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                SetDirectionalStabilityAt(x, y, Direction.Left);
            }

            for (int x = SizeX; x >= 0; x--)
            {
                SetDirectionalStabilityAt(x, y, Direction.Right);
            }
        }
    }

    public override void CalculateAllNeighboursBitmask()
    {
        Util.IterateXY(SizeX, SizeY, CalculateNeighboursBitmaskAt);
    }
}
