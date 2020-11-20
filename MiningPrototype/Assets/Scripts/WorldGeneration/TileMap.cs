using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-100)]
public class TileMap : Singleton<TileMap>, ISavable
{
    [ReadOnly]
    [SerializeField] string saveID = Util.GenerateNewSaveGUID();

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

    private int sizeX, sizeY;
    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }

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
                        float timeLeft = tile.Stability - generationSettings.CollapseThreshhold;
                        AddUnstableTile(loc, timeLeft);
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
        return TilesData.GetTileInfo(type);
    }

    public void AddTileToCheckForStability(Vector2Int tileLoc)
    {
        tilesToStabilityCheck.Push(tileLoc);
    }

    private void AddUnstableTile(Vector2Int unstableTile, float timeLeftToCrumble)
    {
        if (!unstableTiles.Contains(unstableTile))
        {

            unstableTiles.Add(unstableTile);
            var go = Instantiate(mapSettings.CrumbleEffects, new Vector3(unstableTile.x + 0.5f, unstableTile.y), quaternion.identity, transform);
            go.GetComponent<CrumblingParticle>().SetDuration(timeLeftToCrumble);

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

        sizeX = generationSettings.SizeX;
        sizeY = generationSettings.SizeY;
        generator = new TileMapGenerator(this, generationSettings);
    }

    public void InitMap(int sizeX, int sizeY)
    {
        map = new Tile[sizeX, sizeY];
        receiverMap = new ITileUpdateReceiver[sizeX, sizeY];
    }

    public bool IsAirAt(int x, int y)
    {
        return GetTileInfo(this[x, y].Type).AirLike;
    }

    public bool CanTarget(int x, int y)
    {
        var info = GetTileInfo(GetTileAt(x, y).Type);
        return info.Targetable;
    }

    public bool IsBlockAt(int x, int y)
    {
        return !IsAirAt(x, y);
    }

    public bool IsNeighbourAt(int x, int y)
    {
        return GetTileInfo(this[x, y].Type).CountsAsNeighbour;
    }


    public Tile GetTileAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
        {
            if (x < 0)
            {
                return GetTileAt(x + SizeX, y); //Our map is topologically a cylinder 
            }
            else if (x >= sizeX)
            {
                return GetTileAt(x - SizeX, y);
            }
            return Tile.Air;
        }
        else
        {
            return map[x, y];
        }

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
        Util.IterateXY(SizeX, SizeY, UpdateVisualsAt);
    }


    private void UpdateVisualsAt(int x, int y)
    {
        var tile = GetVisualTileFor(x, y);
        var destTile = GetVisualDestructableOverlayFor(x, y);
        var oreTile = GetVisualOverlayTileFor(x, y);

        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), destTile);
        oreTilemap.SetTile(new Vector3Int(x, y, 0), oreTile);

        if (x < generationSettings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(SizeX + x, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(SizeX + x, y, 0), oreTile);
        }

        if (x > SizeX - generationSettings.MirroringAmount)
        {
            tilemap.SetTile(new Vector3Int(x - SizeX, y, 0), tile);
            damageOverlayTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), destTile);
            oreTilemap.SetTile(new Vector3Int(x - SizeX, y, 0), oreTile);
        }
    }

    public bool IsOutOfBounds(int x, int y)
    {
        return (x < 0 || y < 0 || x >= SizeX || y >= SizeY);
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

            GUI.DrawTexture(new Rect(10, 10, SizeX * 4, SizeY * 4), stabilityDebugTexture);
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
        stabilityDebugTexture = new Texture2D(SizeX, SizeY);
        stabilityDebugTexture.filterMode = FilterMode.Point;

        Util.IterateXY(SizeX, SizeY, (x, y) => stabilityDebugTexture.SetPixel(x, y, TileMapHelper.StabilityToColor(GetTileAt(x, y).Stability)));
        stabilityDebugTexture.Apply();
    }

    public SaveData ToSaveData()
    {
        TileMapSaveData saveData = new TileMapSaveData();
        saveData.GUID = saveID;

        saveData.Map = map;
        Debug.Log("Saving with ID: " + saveData.GUID);
        return saveData;
    }

    public void Load(SaveData data)
    {
        if (data is TileMapSaveData saveData)
        {
            map = saveData.Map;
            UpdateVisuals();
        }
        else
        {
            Debug.LogError("Wrong SaveData received");
        }

    }

    public string GetSaveID()
    {
        return saveID;
    }
}

[System.Serializable]
public class TileMapSaveData : SaveData
{
    public Tile[,] Map;

}