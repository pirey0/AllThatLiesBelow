using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileMap : MonoBehaviour
{
    [SerializeField] Tilemap tilemap, damageOverlayTilemap, oreTilemap;

    [SerializeField] TileMapSettings mapSettings;
    [SerializeField] GenerationSettings generationSettings;


    [Header("Debug")]
    [SerializeField] bool drawStabilityTexture;
    [SerializeField] bool drawStabilityGizmos;
    [SerializeField] int stabilityGizmosSize;
    [SerializeField] PlayerController player;

    Tile[,] map;
    TileMapGenerator generator;
    Texture2D stabilityDebugTexture;

    private int size;
    public int Size { get => size; }

    public Tile this[int x, int y]
    {
        get => GetTileAt(x, y);
        set => SetMapRawAt(x, y, value);
    }

    private void Start()
    {
        tilemap.GetComponent<GridElement>()?.Setup(this);
        damageOverlayTilemap.GetComponent<GridElement>()?.Setup(this);
        oreTilemap.GetComponent<GridElement>()?.Setup(this);

        RunCompleteGeneration();
    }

    [Button]
    private void RunCompleteGeneration()
    {
        Setup();
        generator.RunCompleteGeneration();
        UpdateVisuals();
    }

    private void Setup()
    {
        if (mapSettings == null || generationSettings == null)
        {
            Debug.LogError("TileMap settings missing.");

            Destroy(this);
            return;
        }

        size = generationSettings.Size;
        generator = new TileMapGenerator(this, generationSettings);
    }

    public void InitMap(int sizeX, int sizeY)
    {
        map = new Tile[sizeX, sizeY];
    }

    public bool IsAirAt(int x, int y)
    {
        return GetTileAt(x, y).Type == TileType.Air;
    }

    public bool IsBlockAt(int x, int y)
    {
        return GetTileAt(x, y).Type != TileType.Air;
    }

    public Tile GetTileAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return Tile.Air;

        return map[x, y];
    }

    public bool DamageAt(int x, int y, float amount)
    {
        if (IsOutOfBounds(x, y))
            return false;

        Tile t = GetTileAt(x, y);
        t.TakeDamage(amount);

        //
        if (t.Damage > 10)
        {
            BreakBlock(x, y, t);

            return true;
        }
        else
        {
            SetMapAt(x, y, t, updateProperties: false, updateVisuals: true);
            return false;
        }
    }

    private void BreakBlock(int x, int y, Tile t)
    {
        CarveAt(x, y);

        ItemType itemType = ItemType.ROCKS;

        switch (t.Type)
        {
            case TileType.Gold:
                itemType = ItemType.GOLD;
                break;

            case TileType.Copper:
                itemType = ItemType.COPPER;
                break;
        }

        InventoryManager.PlayerCollects(itemType, 1);
    }

    public void CarveAt(int x, int y)
    {
        //Debug.Log("Try Carve " + x + " / " + y);
        SetMapAt(x, y, Tile.Air);
    }

    public void PlaceAt(int x, int y)
    {
        Debug.Log("Try Place " + x + " / " + y);
        SetMapAt(x, y, Tile.Make(TileType.Stone));
    }

    private void SetMapRawAt(int x, int y, Tile tile)
    {
        if (IsOutOfBounds(x, y))
            return;

        map[x, y] = tile;
    }

    public void SetMapAt(int x, int y, Tile value, bool updateProperties = true, bool updateVisuals = true)
    {
        if (IsOutOfBounds(x, y))
            return;

        map[x, y] = value;

        if (updateProperties)
        {
            generator.UpdatePropertiesAt(x, y);
        }

        if (updateVisuals)
        {
            UpdateVisualsAt(x, y);
            foreach (var nIndex in TileMapHelper.GetNeighboursIndiciesOf(x, y))
            {
                UpdateVisualsAt(nIndex.x, nIndex.y);
            }
        }
    }

    void UpdateVisuals()
    {
        tilemap.ClearAllTiles();
        damageOverlayTilemap.ClearAllTiles();
        oreTilemap.ClearAllTiles();
        Util.IterateXY(Size, UpdateVisualsAt);
    }


    private void UpdateVisualsAt(int x, int y)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), GetVisualTileFor(x, y));
        damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), GetVisualDestructableOverlayFor(x, y));
        oreTilemap.SetTile(new Vector3Int(x, y, 0), GetVisualOreTileFor(x, y));

    }

    public bool IsOutOfBounds(int x, int y)
    {
        return (x < 0 || y < 0 || x >= Size || y >= Size);
    }

    private TileBase GetVisualTileFor(int x, int y)
    {
        Tile tile = GetTileAt(x, y);

        if (IsOutOfBounds(x, y) || IsAirAt(x, y))
            return null;

        if (tile.Type == TileType.Snow)
        {
            return Util.PseudoRandomValue(x, y) > 0.5f ? mapSettings.SnowTile1 : mapSettings.SnowTile2;
        }

        int tileIndex = Util.BITMASK_TO_TILEINDEX[tile.NeighbourBitmask];

        //Casual random tile
        if (tileIndex == 46)
        {
            tileIndex = Util.PseudoRandomValue(x, y) > 0.5f ? 46 : 0;
        }

        return mapSettings.GroundTiles[tileIndex];
    }

    private TileBase GetVisualDestructableOverlayFor(int x, int y)
    {
        var t = GetTileAt(x, y);
        return mapSettings.DamageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    private TileBase GetVisualOreTileFor(int x, int y)
    {
        var t = GetTileAt(x, y);
        if ((int)t.Type < 2 || (int)t.Type == 4)
            return null;

        return mapSettings.OreTiles[(int)t.Type - 2];
    }

    private void OnGUI()
    {
        if (drawStabilityTexture)
        {
            if (stabilityDebugTexture == null)
                UpdateDebugTextures();

            GUI.DrawTexture(new Rect(10, 10, Size * 4, Size * 4), stabilityDebugTexture);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawStabilityGizmos || map == null)
            return;

        for (int y = -stabilityGizmosSize; y < stabilityGizmosSize; y++)
        {
            for (int x = -stabilityGizmosSize; x < stabilityGizmosSize; x++)
            {
                Vector2Int pos = player.GetPositionInGrid() + new Vector2Int(x, y);
                Gizmos.color = TileMapHelper.StabilityToColor(GetTileAt(pos.x, pos.y).Stability);
                Gizmos.DrawCube((Vector3Int)pos + new Vector3(0.5f, 0.5f), new Vector3(1, 1, 0));
            }
        }
    }

    [Button(null, EButtonEnableMode.Playmode)]
    private void UpdateDebugTextures()
    {
        stabilityDebugTexture = new Texture2D(Size, Size);
        stabilityDebugTexture.filterMode = FilterMode.Point;

        Util.IterateXY(Size, (x, y) => stabilityDebugTexture.SetPixel(x, y, TileMapHelper.StabilityToColor(GetTileAt(x, y).Stability)));
        stabilityDebugTexture.Apply();
    }


}
