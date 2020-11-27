using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class MapHelper
{
    public static byte DirectNeighboursSolidBitMap { get => 0b01011010; }

    public static Vector2Int[] GetNeighboursIndiciesOf(int x, int y)
    {
        return new Vector2Int[]
        {
            new Vector2Int(x-1,y+1),
            new Vector2Int(x,y+1),
            new Vector2Int(x+1,y+1),
            new Vector2Int(x-1,y),
            new Vector2Int(x+1,y),
            new Vector2Int(x-1,y-1),
            new Vector2Int(x,y-1),
            new Vector2Int(x+1,y-1)
        };
    }

    public static Vector2Int[] GetDirectNeighboursIndiciesOf(int x, int y)
    {
        return new Vector2Int[]
        {
            new Vector2Int(x,y+1),
            new Vector2Int(x-1,y),
            new Vector2Int(x+1,y),
            new Vector2Int(x,y-1)
        };
    }


    public static bool HasLineOfSight(RuntimeProceduralMap map, Vector2Int start, Vector2Int end, bool debugVisualize = false)
    {
        Vector2Int current = start;

        while (current != end)
        {
            bool blocked = map.IsBlockAt(current.x, current.y);

            if (blocked)
            {
                if (debugVisualize)
                    Debug.DrawLine((Vector3Int)current, (Vector3Int)end, Color.red, 1);
                return false;
            }

            Vector2Int offset = StepTowards(current, end);
            if (debugVisualize)
                Debug.DrawLine((Vector3Int)current, (Vector3Int)(current + offset), Color.yellow, 1f);
            current += offset;
        }

        return true;
    }

    public static Vector2Int StepTowards(Vector2Int current, Vector2Int end)
    {
        Vector2Int delta = end - current;
        Vector2Int offset;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            offset = new Vector2Int((int)Mathf.Sign(delta.x), 0);
        else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
            offset = new Vector2Int(0, (int)Mathf.Sign(delta.y));
        else
            offset = new Vector2Int((int)Mathf.Sign(delta.x), (int)Mathf.Sign(delta.y));

        return offset;
    }

    public static Vector3 StepTowards(Vector3 current, Vector3 end)
    {
        Vector3 delta = end - current;
        Vector3 offset;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            offset = new Vector3((int)Mathf.Sign(delta.x), 0);
        else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
            offset = new Vector3(0, (int)Mathf.Sign(delta.y));
        else
            offset = new Vector3((int)Mathf.Sign(delta.x), (int)Mathf.Sign(delta.y));

        return offset;
    }

    public static Vector3 GetWorldLocationOfFreeFaceFromSource(RuntimeProceduralMap map, Vector2Int target, Vector2Int source)
    {
        Vector2Int disp = source - target;

        if (Mathf.Abs(disp.x) > Mathf.Abs(disp.y))
        {
            bool xAir = map.IsAirAt(target.x + (int)Mathf.Sign(disp.x), target.y);
            if (xAir)
                return (Vector3Int)target + new Vector3((int)Mathf.Sign(disp.x) * 0.5f + 0.5f, 0.5f, 0);
            else
                return (Vector3Int)target + new Vector3(0.5f, (int)Mathf.Sign(disp.y) * 0.5f + 0.5f, 0);
        }
        else
        {
            bool yAir = map.IsAirAt(target.x, target.y + (int)Mathf.Sign(disp.y));
            if (yAir)
                return (Vector3Int)target + new Vector3(0.5f, (int)Mathf.Sign(disp.y) * 0.5f + 0.5f, 0);
            else
                return (Vector3Int)target + new Vector3((int)Mathf.Sign(disp.x) * 0.50f + 0.5f, 0.5f, 0);
        }

    }

    public static Vector2Int GetMiningTarget(RuntimeProceduralMap map, Vector2Int current, Vector2Int end)
    {
        Vector2Int? lastTargetable = null;
        while (current != end)
        {
            var t = map[current];
            var info = TilesData.GetTileInfo(t.Type);

            if (info.TargetPriority)
            {
                return current;
            }
            else if (info.Targetable)
            {
                lastTargetable = current;
            }

            current += StepTowards(current, end);
        }

        var endT = map[end];
        var endInfo = TilesData.GetTileInfo(endT.Type);

        if (endInfo.TargetPriority)
            return end;
        else
            return lastTargetable.HasValue ? lastTargetable.Value : end;
    }
    public static Color StabilityToColor(float stability)
    {
        float v = stability / 100;
        if (stability >= 0)
            return new Color(v, v, v, 1);
        else
            return Color.black;
    }

    public static bool OnEdgeOfMap(RuntimeProceduralMap map, Vector2Int position)
    {
        if (position.x == 0 || position.y == 0 || position.x == map.SizeX - 1 || position.y == map.SizeY - 1)
            return true;

        return false;
    }

    public static bool IsAllAirAt(RuntimeProceduralMap map, Vector2Int[] locations)
    {

        foreach (var loc in locations)
        {
            if (!map.IsAirAt(loc.x, loc.y))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsAllBlockAt(RuntimeProceduralMap map, Vector2Int[] locations)
    {

        foreach (var loc in locations)
        {
            if (!map.IsBlockAt(loc.x, loc.y))
            {
                return false;
            }
        }
        return true;
    }

    public static int AirTileCountAbove(RuntimeProceduralMap map, Vector2Int coordinate)
    {
        int count = 0;
        while (!map.IsOutOfBounds(coordinate.x, coordinate.y))
        {
            if (map.IsAirAt(coordinate.x, coordinate.y) || map[coordinate].Type == TileType.CollapsableEntity || map[coordinate].Type == TileType.FloatingEntity)
            {
                coordinate.y++;
                count++;
            }
            else
            {
                break;
            }

        }
        return count;
    }

    public static BaseMapSaveData LoadMapSaveDataFromTextAsset(TextAsset asset)
    {
        using (var memStream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            memStream.Write(asset.bytes, 0, asset.bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            return (BaseMapSaveData)formatter.Deserialize(memStream);
        }
    }

    public static void FloodFill(BaseMap map, TileType target, TileType replacement, int x, int y)
    {
        if (target == replacement)
            return;

        if (map.IsSetup())
            RecursiveFloodFill(map, target, replacement, x, y);

        if (map is RenderedMap rm)
        {
            rm.CalculateAllNeighboursBitmask();
            rm.CalculateStabilityAll();
        }

        map.UpdateAllVisuals();
    }

    private static void RecursiveFloodFill(BaseMap map, TileType target, TileType replacement, int x, int y)
    {
        if (map.IsOutOfBounds(x, y) || map[x, y].Type != target)
            return;

        map[x, y] = Tile.Make(replacement);

        RecursiveFloodFill(map, target, replacement, x + 1, y);
        RecursiveFloodFill(map, target, replacement, x - 1, y);
        RecursiveFloodFill(map, target, replacement, x, y + 1);
        RecursiveFloodFill(map, target, replacement, x, y - 1);
    }
}
