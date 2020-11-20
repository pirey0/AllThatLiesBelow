using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-100)]
public class Map : MonoBehaviour, ISavable
{
    private static Map instance;

    [ReadOnly]
    [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] int sizeX, sizeY;
    [SerializeField] bool updateUnstable;

    [SerializeField] MapSettings mapSettings;
    [SerializeField] GenerationSettings generationSettings;
    [SerializeField] Transform entitiesParent;
    [SerializeField] MapRenderer renderer;

    [SerializeField] bool createOwnData;
    [SerializeField] bool runGeneration;

    [SerializeField] TileMapData data;

    [SerializeField] bool debug;

    bool runtime = false;
    ITileUpdateReceiver[,] receiverMap;
    TileMapGenerator generator;

    List<Vector2Int> unstableTiles = new List<Vector2Int>();
    List<GameObject> unstableTilesEffects = new List<GameObject>();
    Stack<Vector2Int> tilesToStabilityCheck = new Stack<Vector2Int>();

    public event System.Action<MirrorState> MirrorSideChanged;

    public static Map Instance { get => instance; }
    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }
    public GenerationSettings GenerationSettings { get => generationSettings; }
    public MapSettings Settings { get => mapSettings; }
    public TileMapData Data { get => data; }

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

    private void Awake()
    {
        if(Instance != this && instance != null)
        {
            return;
        }

        if (createOwnData)
            data = ScriptableObject.CreateInstance<TileMapData>();
        else
            data = ScriptableObject.Instantiate(data);


        runtime = true;

        SelectThis();

        if (runGeneration)
            RunCompleteGeneration();

        if (updateUnstable)
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
    public void SelectThis()
    {
        instance = this;
        Setup();
    }

    [Button]
    private void RunCompleteGeneration()
    {
        Setup();
        DestroyAllEntities();
        generator.RunCompleteGeneration();
        renderer.UpdateVisuals();
    }

    [Button]
    private void DestroyAllEntities()
    {
        for (int i = entitiesParent.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(entitiesParent.GetChild(i).gameObject);
        }
    }

    [Button]
    private void Clear()
    {
        InitMap(SizeX, SizeY);
        DestroyAllEntities();
        renderer.UpdateVisuals();
    }

    public GameObject InstantiateEntity(GameObject prefab, Vector3 position)
    {
        var go = GameObject.Instantiate(prefab, entitiesParent);
        go.transform.localPosition = position;

        return go;
    }

    public void NotifyMirrorWorldSideChange(MirrorState newState)
    {
        MirrorSideChanged?.Invoke(newState);
    }

    public enum MirrorState { Center, Right, Left };

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
        renderer.Setup(this);
        if (mapSettings == null || generationSettings == null)
        {
            Debug.LogError("TileMap settings missing.");
            return;
        }
        generator = new TileMapGenerator(this, generationSettings);
        receiverMap = new ITileUpdateReceiver[sizeX, sizeY];
    }

    public void InitMap(int sizeX, int sizeY)
    {
        data.Initialize(sizeX, sizeY);
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
        WrapXIfNecessary(ref x);

        if (IsOutOfBounds(x, y))
        {
            return Tile.Air;
        }
        else
        {
            return data[x, y];
        }

    }

    public bool DamageAt(int x, int y, float amount, bool playerCaused)
    {
        WrapXIfNecessary(ref x);
        if (IsOutOfBounds(x, y))
        {
            return false;
        }

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

        data[x, y] = tile;

        if (debug)
            Util.DebugDrawTile(new Vector2Int(x, y), Color.white, 0.5f);
    }

    public void SetMapAt(int x, int y, Tile value, TileUpdateReason reason, bool updateProperties = true, bool updateVisuals = true)
    {
        if (IsOutOfBounds(x, y))
            return;

        var prev = data[x, y];
        data[x, y] = value;

        if (updateProperties)
        {
            generator.UpdatePropertiesAt(x, y);

            if (prev.Type != value.Type && runtime)
            {
                receiverMap[x, y]?.OnTileUpdated(x, y, reason);
                receiverMap[x, y] = null;
            }
        }

        if (updateVisuals)
        {
            renderer.UpdateVisualsAt(x, y);
            foreach (var nIndex in TileMapHelper.GetNeighboursIndiciesOf(x, y))
            {
                renderer.UpdateVisualsAt(nIndex.x, nIndex.y);
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

    public void WrapXIfNecessary(ref int x)
    {
        if (x < 0)
            x += SizeX;
        else if (x >= sizeX)
            x -= sizeX;
    }

    public bool IsOutOfBounds(int x, int y)
    {
        return (x < 0 || y < 0 || x >= SizeX || y >= SizeY);
    }

    private void OnDrawGizmos()
    {
        float x = 10;
        float y = 10;

        Gizmos.DrawLine(new Vector3(0, -y), new Vector3(0, SizeY + y));
        Gizmos.DrawLine(new Vector3(SizeX, -y), new Vector3(SizeX, SizeY + y));

        Gizmos.DrawLine(new Vector3(-x, 0), new Vector3(SizeX + x, 0));
        Gizmos.DrawLine(new Vector3(-x, SizeY), new Vector3(SizeX + x, SizeY));
    }

    public SaveData ToSaveData()
    {
        TileMapSaveData saveData = new TileMapSaveData();
        saveData.GUID = saveID;

        saveData.Map = data.Map;
        Debug.Log("Saving with ID: " + saveData.GUID);
        return saveData;
    }

    public void Load(SaveData data)
    {
        if (data is TileMapSaveData saveData)
        {
            this.data.Map = saveData.Map;
            renderer.UpdateVisuals();
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

    public void LoadFromMap(TileMapData loadedData, int xOffset, int yOffset)
    {
        Util.IterateXY(loadedData.SizeX, loadedData.SizeY, (x, y) => LoadFromMapAt(loadedData, x, y, xOffset, yOffset));
    }

    private void LoadFromMapAt(TileMapData loadedData, int x, int y, int xOffset, int yOffset)
    {
        SetMapAt(x + xOffset, y + yOffset, loadedData[x, y], TileUpdateReason.Generation, updateProperties: true, updateVisuals: true);
    }
}

[System.Serializable]
public class TileMapSaveData : SaveData
{
    public MapArray Map;
}

[System.Serializable]
public class MapArray
{
    [SerializeField] TileMapColumn[] rows;

    public int SizeX;
    public int SizeY;

    public Tile this[int x, int y]
    {
        get => rows[x][y];
        set => rows[x][y] = value;
    }

    public MapArray(int sizeX, int sizeY)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        rows = new TileMapColumn[sizeX];

        for (int i = 0; i < rows.Length; i++)
        {
            rows[i] = new TileMapColumn(sizeY);
        }
    }
}


[System.Serializable]
public class TileMapColumn
{
    public TileMapColumn(int sizeY)
    {
        column = new Tile[sizeY];
    }

    public Tile[] column;

    public Tile this[int i] { get => column[i]; set => column[i] = value; }
}