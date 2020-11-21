using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-100)]
public class Map : StateListenerBehaviour, ISavable
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

    [SerializeField] bool load;
    [SerializeField] TextAsset loadAsset;

    [SerializeField] bool runGeneration;
    [SerializeField] bool debug;

    bool runtime = false;

    Tile[,] map;
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

    public UnityEngine.Object LoadAsset { get => loadAsset; }

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
        if (Instance != this && instance != null)
        {
            return;
        }

        Debug.Log("Created TileMapData scriptable object");


        runtime = true;

        SelectThis();
        if (load)
        {
            LoadFromAsset(loadAsset);
        }
        else
        {
            if (runGeneration)
                RunCompleteGeneration();
        }
    }

    private void OnDestroy()
    {
        map = null;
        receiverMap = null;
        unstableTiles = null;
        unstableTilesEffects = null;
        tilesToStabilityCheck = null;
    }

    protected override void OnStateChanged(GameState.State newState)
    {
        if (newState == GameState.State.Ready)
        {
            if (updateUnstable)
                StartCoroutine(UpdateUnstableTilesRoutine());
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
        tilesToStabilityCheck.Clear(); //clear on real start

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

#if UNITY_EDITOR
    [Button]
    private void Save()
    {
        string path;

        if (loadAsset == null)
        {
            path = UnityEditor.EditorUtility.SaveFilePanel("Map", "Assets/Other/Maps", "NewMap", "bytes");
        }
        else
        {
            path = UnityEditor.AssetDatabase.GetAssetPath(loadAsset);
        }

        if (path != null)
        {
            DurationTracker tracker = new DurationTracker("Map saving");
            BinaryFormatter formatter = new BinaryFormatter();

            var stream = File.Open(path, FileMode.OpenOrCreate);
            formatter.Serialize(stream, ToSaveData());
            stream.Close();
            UnityEditor.AssetDatabase.Refresh();
            path = Util.MakePathRelative(path);
            loadAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            tracker.Stop();
            Debug.Log("Saved at" + path);
        }
    }
#endif

    [Button]
    private void Load()
    {
        DurationTracker tracker = new DurationTracker("Map Loading");
        if (loadAsset != null)
            LoadFromAsset(loadAsset);
        tracker.Stop();
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
        map = new Tile[sizeX, sizeY];
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
            return map[x, y];
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

        saveData.Map = map;
        return saveData;
    }

    public void Load(SaveData data)
    {
        if (data is TileMapSaveData saveData)
        {
            map = saveData.Map;
            sizeX = map.GetLength(0);
            sizeY = map.GetLength(1);
            renderer.UpdateVisuals();
        }
        else
        {
            Debug.LogError("Wrong SaveData received");
        }
    }

    public void LoadFromAsset(TextAsset saveObject)
    {
        using (var memStream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            memStream.Write(saveObject.bytes, 0, saveObject.bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            TileMapSaveData saveData = (TileMapSaveData)formatter.Deserialize(memStream);
            if (saveData != null)
            {
                Load(saveData);
            }
        }

    }

    public string GetSaveID()
    {
        return saveID;
    }

    //Fix
    public void LoadFromMap(Object loadedData, int xOffset, int yOffset)
    {
        //Util.IterateXY(loadedData.SizeX, loadedData.SizeY, (x, y) => LoadFromMapAt(loadedData, x, y, xOffset, yOffset));
    }

    private void LoadFromMapAt(Object loadedData, int x, int y, int xOffset, int yOffset)
    {

        //var t = loadedData.Map[x, y];

        //if (t.Type != TileType.Ignore)
        //    SetMapAt(x + xOffset, y + yOffset, t, TileUpdateReason.Generation, updateProperties: true, updateVisuals: true);
    }
}

[System.Serializable]
public class TileMapSaveData : SaveData
{
    public Tile[,] Map;

    public int SizeX { get => Map.GetLength(0); }
    public int SizeY { get => Map.GetLength(1); }
}