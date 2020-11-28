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
public class BaseMap : StateListenerBehaviour, ISavable
{
    [Header("BaseMap")]
    [ReadOnly]
    [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] int sizeX, sizeY;
    [SerializeField] MapSettings mapSettings;
    [SerializeField] GenerationSettings generationSettings;

    protected Tile[,] map;

    public int SizeX { get => sizeX; }
    public int SizeY { get => sizeY; }
    public GenerationSettings GenerationSettings { get => generationSettings; }
    public MapSettings MapSettings { get => mapSettings; }
    public MapSettings Settings { get => mapSettings; }



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

    protected virtual void OnDestroy()
    {
        map = null;
    }
    public TileInfo GetTileInfo(TileType type)
    {
        return TilesData.GetTileInfo(type);
    }

    protected void InitMap(int newX, int newY)
    {
        map = new Tile[newX, newY];
        sizeX = newX;
        sizeY = newY;
        Util.IterateXY(newX, newY, (x, y) => map[x, y] = Tile.Air);
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

    protected virtual void BreakBlock(int x, int y, Tile t, bool playerCaused)
    {
        SetMapAt(x, y, Tile.Air, TileUpdateReason.Destroy);
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
            UpdatePropertiesAt(x, y, value, prev, reason);
        }

        if (updateVisuals)
        {
            UpdateVisualsAt(x, y);
        }

    }

    public virtual void UpdateAllVisuals()
    {

    }

    public virtual void CalculateStabilityAll()
    {

    }

    public virtual void CalculateVisibilityAll()
    {

    }


    protected virtual void UpdateVisualsAt(int x, int y)
    {
    }

    protected virtual void UpdatePropertiesAt(int x, int y, Tile newTile, Tile previousTile, TileUpdateReason reason)
    {
    }


    public virtual void CalculateAllNeighboursBitmask()
    {
    }


    public void ReplaceAll(TileType target, TileType replacer)
    {
        Util.IterateXY(SizeX, sizeY, (x, y) => ReplaceAt(x, y, target, replacer));
        RefreshAll();
    }

    public virtual void RefreshAll()
    {
        Debug.Log("Map Refresh all");
        CalculateAllNeighboursBitmask();
        CalculateStabilityAll();
        CalculateVisibilityAll();
        CalculateDiscoveredAll();
        UpdateAllVisuals();
    }

    protected virtual void CalculateDiscoveredAll()
    {
        Util.IterateXY(SizeX, SizeY, (x, y) => { Tile t = this[x, y]; t.Discovered = false; this[x, y] = t; });
        PropagateDiscoveryFrom(0, SizeY - 1);
    }

    protected virtual void PropagateDiscoveryFrom(int x, int y)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x,y));
        int max = 100000;
        while (stack.Count > 0 && max-- > 0)
        {
            Vector2Int v2 = stack.Pop();
            Tile t = this[v2];

            if (t.Discovered || IsOutOfBounds(v2.x, v2.y))
                continue;

            t.Discovered = true;
            this[v2.x, v2.y] = t;

            if (IsAirAt(v2.x, v2.y))
            {
                foreach (var n in MapHelper.GetDirectNeighboursIndiciesOf(v2.x, v2.y))
                {
                    stack.Push(n);
                }
            }
        }

        if (max <= 0)
            Debug.LogError("Oveflow");
    }


    private void ReplaceAt(int x, int y, TileType target, TileType replacer)
    {
        if (this[x, y].Type == target)
            SetMapRawAt(x, y, Tile.Make(replacer));
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
        BaseMapSaveData saveData = new BaseMapSaveData();
        saveData.GUID = GetSaveID();

        saveData.Map = map;
        return saveData;
    }

    public virtual void Load(SaveData data)
    {
        if (data is BaseMapSaveData saveData)
        {
            map = saveData.Map;
            sizeX = map.GetLength(0);
            sizeY = map.GetLength(1);
            Setup();
            UpdateAllVisuals();
        }
        else
        {
            Debug.LogError("Wrong SaveData received");
        }
    }

    protected virtual void Setup()
    {

    }

    public void LoadFromAsset(TextAsset saveObject)
    {
        var saveData = MapHelper.LoadMapSaveDataFromTextAsset(saveObject);
        if (saveData != null)
        {
            Load(saveData);
        }
    }

    public virtual string GetSaveID()
    {
        return saveID;
    }

    //Outdated from SO Time
    public void LoadFromMap(TextAsset assetToLoad, int xOffset, int yOffset)
    {
        var data = MapHelper.LoadMapSaveDataFromTextAsset(assetToLoad);

        Util.IterateXY(data.SizeX, data.SizeY, (x, y) => LoadFromMapAt(data, x, y, xOffset, yOffset));
    }

    private void LoadFromMapAt(BaseMapSaveData loadedData, int x, int y, int xOffset, int yOffset)
    {
        var t = loadedData.Map[x, y];

        if (t.Type != TileType.Ignore)
            SetMapAt(x + xOffset, y + yOffset, t, TileUpdateReason.MapLoad, updateProperties: true, updateVisuals: false);
    }

    public bool IsSetup()
    {
        return map != null;
    }

    public int GetLoadPriority()
    {
        return -10;
    }
}


[System.Serializable]
public class BaseMapSaveData : SaveData
{
    public Tile[,] Map;
    public int SizeX { get => Map.GetLength(0); }
    public int SizeY { get => Map.GetLength(1); }
}