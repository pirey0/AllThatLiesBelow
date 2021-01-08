using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RenderedMap : BaseMap
{
    [Header("RenderedMap")]
    [SerializeField] protected Tilemap tilemap;
    [SerializeField] protected Tilemap tilemapShifted, oreTilemap, damageOverlayTilemap, damageOverlayBehind;
    [SerializeField] protected bool debugRendering, showOverlayAlways;

    protected override void Setup()
    {
        base.Setup();
        tilemap.GetComponent<ITileMapElement>()?.Setup(this);
    }

    public override void UpdateAllVisuals()
    {
        tilemap.ClearAllTiles();
        tilemapShifted?.ClearAllTiles();
        damageOverlayTilemap.ClearAllTiles();
        damageOverlayBehind.ClearAllTiles();
        oreTilemap.ClearAllTiles();

        Util.IterateXY(SizeX, SizeY, SetVisualsAt);
    }

    protected override void UpdateVisualsAt(int x, int y)
    {
        SetVisualsAt(x, y);
        foreach (var nIndex in MapHelper.GetNeighboursIndiciesOf(x, y))
            SetVisualsAt(nIndex.x, nIndex.y);

        foreach (var nIndex in MapHelper.Get2ndDegreeNeighboursIndiciesOf(x, y))
            SetVisualsAt(nIndex.x, nIndex.y);
    }

    private void SetVisualsAt(int x, int y)
    {
        WrapXIfNecessary(ref x);

        if (IsOutOfBounds(x, y))
            return;

        var tile = GetVisualTileFor(x, y);
        var destTile = GetVisualDestructableOverlayFor(x, y);
        var oreTile = GetVisualOverlayTileFor(x, y);
        var info = GetTileInfo(this[x, y].Type);

        SetTilemapsAt(x, y, tile, destTile, oreTile, info);

        if (x < Settings.MirroringAmount)
            SetTilemapsAt(SizeX + x, y, tile, destTile, oreTile, info);
        else if (x > SizeX - Settings.MirroringAmount)
            SetTilemapsAt(x - SizeX, y, tile, destTile, oreTile, info);
    }

    private void SetTilemapsAt(int x, int y, TileBase tile, TileBase damage, TileBase ore, TileInfo info)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        tilemapShifted?.SetTile(new Vector3Int(x, y, 0), info.DrawsToShifted ? tile : null);
        oreTilemap.SetTile(new Vector3Int(x, y, 0), ore);
        if (info.DamageIsInBackground)
        {
            damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), null);
            damageOverlayBehind.SetTile(new Vector3Int(x, y, 0), damage);
        }
        else
        {
            damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), damage);
            damageOverlayBehind.SetTile(new Vector3Int(x, y, 0), null);
        }
    }


    private TileBase GetVisualVisibilityTileFor(int x, int y)
    {
        return Settings.VisibilityOverlayTiles[this[x, y].Visibility];
    }

    private TileBase GetVisualTileFor(int x, int y)
    {
        Tile tile = this[x, y];

        if (IsOutOfBounds(x, y) || IsAirAt(x, y))
            return null;



        TileInfo tileInfo = GetTileInfo(tile.Type);
        TileBase tileVis = null;

        if (tileInfo.UseTilesFromOtherInfo && tileInfo.TileSourceInfo != null)
        {
            tileInfo = tileInfo.TileSourceInfo;
        }

        if (tileInfo.Tiles.Length == 48)
        {
            int tileIndex = Util.BITMASK_TO_TILEINDEX[tile.NeighbourBitmask];
            tileVis = tileInfo.Tiles[tileIndex];
        }
        else if (tileInfo.Tiles.Length == 1)
        {
            tileVis = tileInfo.Tiles[0];
        }

        if (tile.Visibility == 2)
        {
            var tv = GetTileInfo(tile.Type).Visibility2Tile;
            if (tv == null) //tile has no own secondary
                tileVis = tileInfo.Visibility2Tile;
            else
                tileVis = tv;
        }

        if (debugRendering && tileInfo.Tiles.Length == 48)
            tileVis = tileInfo.Tiles[47];

        return tileVis;
    }

    private TileBase GetVisualDestructableOverlayFor(int x, int y)
    {
        var t = this[x, y];
        return Settings.DamageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    protected TileBase GetVisualOverlayTileFor(int x, int y)
    {
        var t = this[x, y];
        var tileInfo = GetTileInfo(t.Type);

        if (t.Visibility > 1 && !showOverlayAlways)
            return null;

        return tileInfo.Overlay;
    }

    protected override void UpdatePropertiesAt(int x, int y, Tile newTile, Tile previousTile, TileUpdateReason reason)
    {
        CalculateNeighboursBitmaskAt(x, y);

        foreach (var nIndex in MapHelper.GetNeighboursIndiciesOf(x, y))
            CalculateNeighboursBitmaskAt(nIndex.x, nIndex.y);

        CalculateVisibilityAt(x, y);

        foreach (var nIndex in MapHelper.GetNeighboursIndiciesOf(x, y))
        {
            CalculateVisibilityAt(nIndex.x, nIndex.y);
        }

        foreach (var nIndex in MapHelper.Get2ndDegreeNeighboursIndiciesOf(x, y))
        {
            CalculateVisibilityAt(nIndex.x, nIndex.y);
        }
    }

    private void CalculateVisibilityAt(int x, int y)
    {
        WrapXIfNecessary(ref x);

        if (IsOutOfBounds(x, y))
            return;

        Tile t = this[x, y];

        if (!IsNeighbourAt(x, y))
        {
            t.Visibility = 0;
        }
        else
        {
            int min = 2;
            foreach (var pos in MapHelper.GetDirectNeighboursIndiciesOf(x, y))
            {
                var nVis = this[pos].Visibility;
                if (nVis < min)
                    min = nVis;
            }

            foreach (var pos in MapHelper.GetCornerIndiciesOf(x, y))
            {
                var nVis = this[pos].Visibility;
                if (nVis == 0)
                    min = 0;
            }

            t.Visibility = min + 1;
        }

        this[x, y] = t;
        //Util.DebugDrawTile(new Vector2Int(x, y), t.Visibility==0? Color.white: t.Visibility==1? Color.white : t.Visibility ==2? Color.gray:Color.blue);
    }


    private void CalculateNeighboursBitmaskAt(int x, int y)
    {
        WrapXIfNecessary(ref x);

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


    public override void CalculateVisibilityAll()
    {
        Util.IterateXY(SizeX, SizeY, (x, y) => ResetVisibilityAt(x, y));
        Util.IterateXY(SizeX, SizeY, (x, y) => CalculateVisibilityAt(x, y));
        Util.IterateXY(SizeX, SizeY, (x, y) => CalculateVisibilityAt(x, y)); //two passes for two layers of visibility
    }

    private void ResetVisibilityAt(int x, int y)
    {
        var t = this[x, y];
        if (IsNeighbourAt(x, y))
            t.Visibility = 3;
        else
            t.Visibility = 0;
        this[x, y] = t;
    }

    public override void CalculateAllNeighboursBitmask()
    {
        Util.IterateXY(SizeX, SizeY, CalculateNeighboursBitmaskAt);
    }
}
