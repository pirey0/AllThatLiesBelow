﻿using NaughtyAttributes;
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

    [SerializeField] int sizeX;
    [SerializeField] int sizeY;
    [SerializeField] MapSettings mapSettings;
    [SerializeField] GenerationSettings generationSettings;

    private Tile[,] map;

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

    public bool MapIsNull()
    {
        return map == null;
    }

    protected virtual void OnDestroy()
    {
        map = null;
    }
    public TileInfo GetTileInfo(TileType type)
    {
        return TilesData.GetTileInfo(type);
    }

    public TileInfo GetTileInfoAt(Vector2Int loc)
    {
        return GetTileInfo(this[loc].Type);
    }


    protected void InitMap(int newX, int newY)
    {
        map = new Tile[newX, newY];
        sizeX = newX;
        sizeY = newY;
        Util.IterateXY(newX, newY, (x, y) => map[x, y] = Tile.Air);
    }

    protected void ResizeMap(int newX, int newY)
    {
        var oldMap = map;
        int oldSizeX = sizeX;
        int oldSizeY = sizeY;

        map = new Tile[newX, newY];
        sizeX = newX;
        sizeY = newY;

        Util.IterateXY(newX, newY, (x, y) => map[x, y] = (x < oldSizeX && y < oldSizeY) ? oldMap[x, y] : Tile.Air);
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
        WrapXIfNecessary(ref x);
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

    public virtual bool DamageAt(int x, int y, float amount, DamageType damageType)
    {
        WrapXIfNecessary(ref x);
        if (IsOutOfBounds(x, y))
        {
            return false;
        }

        Tile t = GetTileAt(x, y);
        TileInfo info = GetTileInfo(t.Type);

        if (damageType == DamageType.Mining)
        {
            t.TakeDamage(amount * info.damageMultiplyer);
        }
        else if ((damageType == DamageType.Explosion || damageType == DamageType.Crush) && info.damagedByExplosion)
        {
            t.TakeDamage(amount);
        }


        if (t.Damage > 10)
        {
            BreakBlock(x, y, t, damageType);
            

            return true;
        }
        else
        {
            SetMapAt(x, y, t, TileUpdateReason.VisualUpdate, updateProperties: false, updateVisuals: true);
            return false;
        }
    }

    public enum DamageType { Mining, Explosion, Crush }

    protected virtual void BreakBlock(int x, int y, Tile t, DamageType damageType)
    {
        SetMapAt(x, y, Tile.Air, TileUpdateReason.Destroy);
        if (damageType == DamageType.Mining)
            PropagateDiscoveryFrom(x, y);
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

        var prev = this[x, y];
        this[x, y] = value;

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
        stack.Push(new Vector2Int(x, y));
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

    public Vector2Int? FindUndiscoveredAreaOfSize(int startX, int startY, int width, int height, int maxRange)
    {
        int x = startX;
        int y = startY - height;

        while (y > startY - maxRange && y >= 0)
        {
            if (IsAreaUndiscovered(x, y, width, height, out Vector2Int location))
            {
                return new Vector2Int(x, y);
            }
            else
            {
                x = location.x + 1;
                if (x > startX + maxRange || x + width >= sizeX)
                {
                    x = startX - maxRange;
                    y -= 1;
                }
            }
        }

        return null;
    }

    private bool IsAreaUndiscovered(int px, int py, int width, int height, out Vector2Int loc)
    {
        loc = Vector2Int.zero;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsOutOfBounds(px + x, py + y))
                    return false;

                Util.DebugDrawTile(new Vector2Int(px + x, py + y), Color.yellow, 20);
                if (this[px + x, py + y].Discovered)
                {
                    loc = new Vector2Int(px + x, py + y);

                    return false;
                }
            }
        }

        return true;
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

    public void WrapXIfNecessary(ref Vector2Int v2)
    {
        if (v2.x < 0)
            v2.x += SizeX;
        else if (v2.x >= sizeX)
            v2.x -= sizeX;
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

    public virtual string GetSaveID()
    {
        return "BaseMap";
    }

    public void AdditiveLoad(BaseMapSaveData saveData, int xOffset, int yOffset)
    {
        Util.IterateXY(saveData.SizeX, saveData.SizeY, (x, y) => AdditiveLoadAt(saveData, x, y, xOffset, yOffset));
    }

    protected virtual void AdditiveLoadAt(BaseMapSaveData loadedData, int x, int y, int xOffset, int yOffset)
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