using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileMapHelper
{
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


    public static bool HasLineOfSight(TileMap map, Vector2Int start, Vector2Int end, bool debugVisualize = false)
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

    public static Vector3 GetWorldLocationOfFreeFaceFromSource(TileMap map, Vector2Int target, Vector2Int source)
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

    public static Vector2Int GetClosestSolidBlock(TileMap map, Vector2Int current, Vector2Int end)
    {
        while (current != end)
        {
            if (map.IsBlockAt(current.x, current.y))
                return current;

            current += StepTowards(current, end);
        }
        return end;
    }
    public static Color StabilityToColor(float stability)
    {

        if (stability > 20)
            return Color.white;
        else if (stability > 10)
            return Color.grey;
        else if (stability >= 0)
            return Color.red;
        else
            return Color.black;
    }

    public static bool IsAllAirAt(TileMap map, Vector2Int[] locations)
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

    public static bool IsAllBlockAt(TileMap map, Vector2Int[] locations)
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

    public static int AirTileCountAbove(TileMap map, Vector2Int coordinate)
    {
        int count = 0;
        while (!map.IsOutOfBounds(coordinate.x, coordinate.y))
        {
            coordinate.y += 1;
            if(map.IsAirAt(coordinate.x, coordinate.y))
            {
                count++;
            }
            else
            {
                break;
            }

        }
        return count;
    }

}
