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
    public static Vector2Int[] Get2ndDegreeNeighboursIndiciesOf(int x, int y)
    {
        List<Vector2Int> ns = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++)
        {
            ns.Add(new Vector2Int(x + i, y + 2));
            ns.Add(new Vector2Int(x + i, y + -2));
            ns.Add(new Vector2Int(x + 2, y + i));
            ns.Add(new Vector2Int(x + -2, y + i));
        }
        ns.Add(new Vector2Int(x + -2, y + 2));
        ns.Add(new Vector2Int(x + -2, y + -2));
        ns.Add(new Vector2Int(x + 2, y + 2));
        ns.Add(new Vector2Int(x + 2, y + -2));
        return ns.ToArray();
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

    public static Vector2Int[] GetCornerIndiciesOf(int x, int y)
    {
        return new Vector2Int[]
        {
            new Vector2Int(x+1,y+1),
            new Vector2Int(x-1,y+1),
            new Vector2Int(x+1,y-1),
            new Vector2Int(x-1,y-1)
        };
    }

    public static Vector2Int[] Get2ndDegreeDirectNeighboursIndiciesOf(int x, int y)
    {
        return new Vector2Int[]
        {
            new Vector2Int(x,y+2),
            new Vector2Int(x-2,y),
            new Vector2Int(x+2,y),
            new Vector2Int(x,y-2),
            new Vector2Int(x+1,y+1),
            new Vector2Int(x+1,y-1),
            new Vector2Int(x-1,y-1),
            new Vector2Int(x-1,y+1)
        };
    }

    public static bool HasLineOfSight(RuntimeProceduralMap map, Vector2Int start, Vector2Int end, bool debugVisualize = false)
    {
        Vector3 current = start.AsV3();

        while (current.ToGridPosition() != end)
        {
            Vector2Int cV2 = current.ToGridPosition();

            bool blocked = map.IsBlockAt(cV2.x, cV2.y);

            if (blocked)
            {
                if (debugVisualize)
                    Debug.DrawLine(current, (Vector3Int)end, Color.red, 1);
                return false;
            }

            Vector3 offset = StepTowards(current, end.AsV3() + new Vector3(0.5f,0.5f));
            if (debugVisualize)
                Debug.DrawLine(current, (current + offset), Color.yellow, 1f);
            current += offset;
        }

        return true;
    }

    public static Vector3 StepTowards(Vector3 current, Vector3 end)
    {
        Vector3 delta = end - current;
        return delta.normalized;
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

    public static Vector2Int GetMiningTarget(RuntimeProceduralMap map, Vector3 current, Vector2Int end)
    {

        int counter = 10;
        while (current.ToGridPosition() != end && counter-->0)
        {
            var t = map[current.ToGridPosition()];
            var info = TilesData.GetTileInfo(t.Type);

            if (info.TargetPriority)
            {
                return current.ToGridPosition();
            }

            current += StepTowards(current, end.AsV3() + new Vector3(0.5f, 0.5f));
        }

        var endT = map[end];
        return end;
    }
    public static Color StabilityToColor(float stability)
    {
        float v = stability / 100;
        if (stability >= 0)
            return new Color(v, v, v, 1);
        else
            return Color.black;
    }

    public static Color TileToColor(TileType t)
    {
        switch (t)
        {
            case TileType.Air:
                return Color.white;

            case TileType.Copper:
                return new Color(1, 0.5f, 0); //Orange

            case TileType.Gold:
                return new Color(1, 1, 0); //yellow

            case TileType.Diamond:
                return Color.cyan;

            case TileType.HardStone:
                return Color.gray;
            
            case TileType.Rock:
                return Color.gray;

            case TileType.FillingStone:
                return new Color(0.1f,0.1f,0.1f);


            case TileType.CollapsableEntity:
            case TileType.CollapsableEntityNotNeighbour:
            case TileType.FloatingEntity:
            case TileType.FloatingEntityNotNeighbour:
                return Color.blue;

            default:
                return Color.black;
        }


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
