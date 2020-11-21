using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-100)]
public class MapBase : StateListenerBehaviour, ISavable
{
    [SerializeField] int sizeX, sizeY;
    [SerializeField] MapSettings mapSettings;
    [SerializeField] GenerationSettings generationSettings;

    [SerializeField] bool load;
    [SerializeField] TextAsset loadAsset;

    Tile[,] map;

    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }
    public GenerationSettings GenerationSettings { get => generationSettings; }
    public MapSettings Settings { get => mapSettings; }

    public UnityEngine.TextAsset LoadAsset { get => loadAsset; }

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
        if (load)
            LoadFromAsset(loadAsset);
    }

    private void OnDestroy()
    {
        map = null;
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

    public TileInfo GetTileInfo(TileType type)
    {
        return TilesData.GetTileInfo(type);
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

    protected void SetMapRawAt(int x, int y, Tile tile)
    {
        if (IsOutOfBounds(x, y))
            return;
        map[x, y] = tile;
    }

    public virtual void SetMapAt(int x, int y, Tile value, TileUpdateReason reason, bool updateProperties = true, bool updateVisuals = true)
    {
        if (IsOutOfBounds(x, y))
            return;

        var prev = map[x, y];
        map[x, y] = value;

        if (updateProperties)
        {
            UpdatePropertiesAt(x, y);
        }

        if (updateVisuals)
        {
            UpdateVisualsAt(x, y);
        }

    }

    protected virtual void UpdateVisualsAt(int x, int y)
    {
    }

    protected virtual void UpdatePropertiesAt(int x, int y)
    {
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

    protected virtual void OnDrawGizmos()
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
        saveData.GUID = "";

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

    public virtual string GetSaveID()
    {
        return "UNDEFINED";
    }

    //Outdated from SO Time
    public void LoadFromMap(TextAsset loadedData, int xOffset, int yOffset)
    {
        //Util.IterateXY(loadedData.SizeX, loadedData.SizeY, (x, y) => LoadFromMapAt(loadedData, x, y, xOffset, yOffset));
    }

    private void LoadFromMapAt(TextAsset loadedData, int x, int y, int xOffset, int yOffset)
    {
        //var t = loadedData.Map[x, y];

        //if (t.Type != TileType.Ignore)
        //    SetMapAt(x + xOffset, y + yOffset, t, TileUpdateReason.Generation, updateProperties: true, updateVisuals: true);
    }
}