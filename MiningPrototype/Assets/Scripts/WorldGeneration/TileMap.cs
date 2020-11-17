using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-100)]
public class TileMap : Singleton<TileMap>
{
    [SerializeField] Tilemap tilemap, damageOverlayTilemap, oreTilemap;

    [SerializeField] TileMapSettings mapSettings;
    [SerializeField] GenerationSettings generationSettings;

    [Header("Debug")]
    [SerializeField] bool debug;
    [SerializeField] bool drawStabilityTexture;
    [SerializeField] bool drawStabilityGizmos;
    [SerializeField] int stabilityGizmosSize;
    [SerializeField] PlayerInteractionHandler player;

    Tile[,] map;
    ITileUpdateReceiver[,] receiverMap;
    TileMapGenerator generator;
    Texture2D stabilityDebugTexture;

    List<Vector2Int> unstableTiles = new List<Vector2Int>();
    List<GameObject> unstableTilesEffects = new List<GameObject>();

    Stack<Vector2Int> tilesToStabilityCheck = new Stack<Vector2Int>();

    Dictionary<TileType, bool> IsAirLike;
    Dictionary<TileType, bool> countsAsNeighbour;

    private int size;
    public int Size { get => size; }

    /// <summary>
    /// Use SetTMapAt for full control / visual updates
    /// </summary>
    public Tile this[int x, int y]
    {
        get => GetTileAt(x, y);
        set => SetMapRawAt(x, y, value);
    }

    public Tile this[Vector2Int v]
    {
        get => GetTileAt(v.x, v.y);
        set => SetMapRawAt(v.x, v.y, value);
    }

    private void Start()
    {
        tilemap.GetComponent<ITileMapElement>()?.Setup(this);
        damageOverlayTilemap.GetComponent<ITileMapElement>()?.Setup(this);
        oreTilemap.GetComponent<ITileMapElement>()?.Setup(this);

        RunCompleteGeneration();

        StartCoroutine(UpdateUnstableTilesRoutine());
    }

    private void PopulateLookupDictionaries()
    {
        IsAirLike = new Dictionary<TileType, bool>();

        foreach (var info in mapSettings.TileInfos)
        {
            IsAirLike.Add(info.Type, info.AirLike);
        }

        countsAsNeighbour = new Dictionary<TileType, bool>();
        foreach (var info in mapSettings.TileInfos)
        {
            countsAsNeighbour.Add(info.Type, info.CountsAsNeighbour);
        }
    }

    private void Update()
    {
        if (debug)
        {
            TooltipHandler.Instance?.Display(transform, this[Util.MouseToWorld().ToGridPosition()].ToString(), "");
        }
    }

    private IEnumerator UpdateUnstableTilesRoutine()
    {
        int i = 0;
        while (true)
        {
            //loop through tiles to check
            while (tilesToStabilityCheck.Count > 0)
            {
                var loc = tilesToStabilityCheck.Pop();
                var tile = this[loc];
                var info = GetTileInfo(tile.Type);
                if (info.StabilityAffected)
                {
                    if (tile.Stability <= generationSettings.UnstableThreshhold)
                    {
                        AddUnstableTile(loc);
                    }
                }
            }

            //iterate through unstable tiles
            if (unstableTiles.Count == 0)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }
            else if (i >= unstableTiles.Count)
            {
                yield return new WaitForSeconds(1f);
                i = 0;
            }

            var t = this[unstableTiles[i]];
            int x = unstableTiles[i].x;
            int y = unstableTiles[i].y;

            if (t.Stability <= generationSettings.CollapseThreshhold)
            {
                RemoveUnstableTileAt(i);
                var info = GetTileInfo(t.Type);
                if (info.NotifiesInsteadOfCrumbling)
                {
                    receiverMap[x, y]?.OnTileCrumbleNotified(x, y);
                }
                else
                {
                    if (t.Type != TileType.Air)
                        generator.CollapseAt(x, y, updateVisuals: true);
                }
            }
            else if (t.Stability <= generationSettings.UnstableThreshhold)
            {
                t.ReduceStabilityBy(1);

                SetMapRawAt(x, y, t);
                i++;
            }
            else
            {
                RemoveUnstableTileAt(i);
            }
        }
    }


    [Button]
    private void RunCompleteGeneration()
    {
        Setup();
        generator.RunCompleteGeneration();
        UpdateVisuals();
    }

    public TileInfo GetTileInfo(TileType type)
    {
        if ((int)type < mapSettings.TileInfos.Length)
            return mapSettings.TileInfos[(int)type];

        return null;
    }

    public void AddTileToCheckForStability(Vector2Int tileLoc)
    {
        tilesToStabilityCheck.Push(tileLoc);
    }

    private void AddUnstableTile(Vector2Int unstableTile)
    {
        if (!unstableTiles.Contains(unstableTile))
        {

            unstableTiles.Add(unstableTile);
            var go = Instantiate(mapSettings.CrumbleEffects, new Vector3(unstableTile.x + 0.5f, unstableTile.y), quaternion.identity, transform);
            unstableTilesEffects.Add(go);
        }

    }

    public void RemoveUnstableTileAt(int i)
    {
        unstableTiles.RemoveAt(i);

        var go = unstableTilesEffects[i];
        Destroy(go);
        unstableTilesEffects.RemoveAt(i);
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
        PopulateLookupDictionaries();
    }

    public void InitMap(int sizeX, int sizeY)
    {
        map = new Tile[sizeX, sizeY];
        receiverMap = new ITileUpdateReceiver[sizeX, sizeY];
    }

    public bool IsAirAt(int x, int y)
    {
        return IsAirLike[GetTileAt(x, y).Type];
    }

    public bool CanTarget(int x, int y)
    {
        var info = GetTileInfo(GetTileAt(x, y).Type);
        return info.Targetable;
    }

    public bool IsBlockAt(int x, int y)
    {
        return !IsAirLike[GetTileAt(x, y).Type];
    }

    public bool IsNeighbourAt(int x, int y)
    {
        return countsAsNeighbour[GetTileAt(x, y).Type];
    }


    public Tile GetTileAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return Tile.Air;

        return map[x, y];
    }

    public bool DamageAt(int x, int y, float amount, bool playerCaused)
    {
        if (IsOutOfBounds(x, y))
            return false;

        Tile t = GetTileAt(x, y);
        TileInfo info = GetTileInfo(t.Type);

        t.TakeDamage(amount * info.damageMultiplyer);

        if (t.Damage > 10)
        {
            BreakBlock(x, y, t, playerCaused);

            return true;
        }
        else
        {
            SetMapAt(x, y, t, TileUpdateReason.VisualUpdate, updateProperties: false, updateVisuals: true);
            return false;
        }
    }


    private void BreakBlock(int x, int y, Tile t, bool playerCaused)
    {
        SetMapAt(x, y, Tile.Air, TileUpdateReason.Destroy);

        if (playerCaused)
        {
            TileInfo info = GetTileInfo(t.Type);

            if (info.ItemToDrop != ItemType.None)
                InventoryManager.PlayerCollects(info.ItemToDrop, UnityEngine.Random.Range(1, ProgressionHandler.Instance.ExtraDrop));
        }

    }

    public void PlaceAt(int x, int y, Tile t)
    {
        //Debug.Log("Try Place " + x + " / " + y);
        SetMapAt(x, y, t, TileUpdateReason.Place);
    }

    private void SetMapRawAt(int x, int y, Tile tile)
    {
        if (IsOutOfBounds(x, y))
            return;

        map[x, y] = tile;

        if (debug)
            Util.DebugDrawTile(new Vector2Int(x, y), Color.white, 0.5f);
    }

    public void SetMapAt(int x, int y, Tile value, TileUpdateReason reason, bool updateProperties = true, bool updateVisuals = true)
    {
        if (IsOutOfBounds(x, y))
            return;

        var prev = map[x, y];
        map[x, y] = value;

        if (updateProperties)
        {
            generator.UpdatePropertiesAt(x, y);

            if (prev.Type != value.Type)
            {
                receiverMap[x, y]?.OnTileUpdated(x, y, reason);
                receiverMap[x, y] = null;
            }
        }

        if (updateVisuals)
        {
            UpdateVisualsAt(x, y);
            foreach (var nIndex in TileMapHelper.GetNeighboursIndiciesOf(x, y))
            {
                UpdateVisualsAt(nIndex.x, nIndex.y);
            }
        }

        if (debug)
        {
            switch (reason)
            {
                case TileUpdateReason.VisualUpdate:
                    Util.DebugDrawTile(new Vector2Int(x, y), Color.yellow, 1);
                    break;

                case TileUpdateReason.Uncarve:
                case TileUpdateReason.Carve:
                case TileUpdateReason.Place:
                    Util.DebugDrawTile(new Vector2Int(x, y), Color.blue, 1);
                    break;

                case TileUpdateReason.Destroy:
                case TileUpdateReason.Collapse:
                    Util.DebugDrawTile(new Vector2Int(x, y), Color.red, 1);
                    break;
            }
        }
            
    }

    public void SetReceiverMapAt(int x, int y, ITileUpdateReceiver receiver)
    {
        if (IsOutOfBounds(x, y))
            return;

        receiverMap[x, y] = receiver;
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
        oreTilemap.SetTile(new Vector3Int(x, y, 0), GetVisualOverlayTileFor(x, y));

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
        var t = GetTileAt(x, y);
        return mapSettings.DamageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    private TileBase GetVisualOverlayTileFor(int x, int y)
    {
        var t = GetTileAt(x, y);
        var info = GetTileInfo(t.Type);
        return info.Overlay;
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