
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Util
{
    public static readonly Dictionary<int, int> BITMASK_TO_TILEINDEX = new Dictionary<int, int>()
    {{2, 1 },{ 8, 2 }, {10, 3 }, {11, 4 }, {16, 5 }, {18, 6 }, { 22, 7 },
        { 24, 8 }, {26, 9 }, {27, 10 }, {30, 11 }, {31, 12 }, {64, 13 },{ 66 , 14},
        { 72 , 15},{ 74 , 16},{ 75 , 17},{ 80 , 18},{ 82 , 19},{ 86 , 20},{ 88 , 21},
        { 90 , 22},{ 91 , 23},{ 94 , 24},{ 95 , 25},{ 104 , 26},{ 106 , 27},{ 107 , 28},
        { 120 , 29},{ 122 , 30},{ 123 , 31},{ 126 , 32},{ 127 , 33},{ 208 , 34},{ 210 , 35},
        { 214 , 36},{ 216 , 37},{ 218 , 38},{ 219 , 39},{ 222 , 40},{ 223 , 41},{ 248 , 42},
        { 250 , 43},{ 251 , 44},{ 254 , 45},{ 255 , 46},{ 0 , 47 } };

    public static void IterateXY(int size, System.Action<int, int> action)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                action(x, y);
            }
        }
    }

    public static void IterateXY(int sizeX, int sizeY, System.Action<int, int> action)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                action(x, y);
            }
        }
    }

    public static void IterateX(int size, System.Action<int> action)
    {
        for (int i = 0; i < size; i++)
        {
            action(i);
        }
    }

    public static float PseudoRandomValue(float x, float y)
    {
        return (float)(Mathf.Sin(Vector2.Dot(new Vector2(x, y), new Vector2(12.9898f, 78.233f))) * 43758.5453) % 1;
    }

    public static int RandomInVector(Vector2Int vector)
    {
        return Random.Range(vector.x, vector.y);
    }

    public static Vector2Int RandomDirection()
    {
        int value = Random.Range(0, 4);

        switch (value)
        {
            case 0:
                return Vector2Int.right;
            case 1:
                return Vector2Int.down;
            case 2:
                return Vector2Int.left;
            default:
                return Vector2Int.up;
        }
    }

    public static Vector2Int RandomDirectionWeighted(int horizontal, int vertical)
    {
        int rnd = Random.Range(0, 2);

        int dirIndex = RandomFromProportionalGroups(new int[] { horizontal, vertical });

        if (dirIndex == 0 && rnd == 0)
            return Vector2Int.right;
        else if (dirIndex == 0 && rnd != 0)
            return Vector2Int.left;
        else if (dirIndex == 1 && rnd == 0)
            return Vector2Int.down;
        else
            return Vector2Int.up;
    }

    public static int RandomFromProportionalGroups(int[] groups)
    {
        var rnd = Random.Range(0, groups.Sum());
        int counter = 0;
        for (int i = 0; i < groups.Length; i++)
        {
            counter += groups[i];
            if (counter > rnd)
                return i;
        }
        throw new System.Exception("Impossibility reached");
    }

    public static Vector2Int ToGridPosition(this Vector3 vector)
    {
        return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
    }

    public static float Abs(this float f)
    {
        return Mathf.Abs(f);
    }

    public static float Sign(this float f)
    {
        return Mathf.Sign(f);
    }

    public static Vector3 AsV3(this Vector2Int v)
    {
        return new Vector3(v.x, v.y);
    }

    public static void DebugDrawTile(Vector2Int location)
    {
        DebugDrawTile(location, Color.white, 1);
    }

    public static void DebugDrawTile(Vector2Int location, Color color, float duration = 1)
    {
        Debug.DrawLine(location.AsV3(), location.AsV3() + Vector3.up, color, duration);
        Debug.DrawLine(location.AsV3(), location.AsV3() + Vector3.right, color, duration);
        Debug.DrawLine(location.AsV3() + Vector3.up, location.AsV3() + Vector3.up + Vector3.right, color, duration);
        Debug.DrawLine(location.AsV3() + Vector3.right, location.AsV3() + Vector3.up + Vector3.right, color, duration);
    }

    public static void DebugDrawTileCrossed(Vector2Int location, Color color, float duration = 1)
    {
        Debug.DrawLine(location.AsV3(), location.AsV3() + Vector3.up, color, duration);
        Debug.DrawLine(location.AsV3(), location.AsV3() + Vector3.right, color, duration);
        Debug.DrawLine(location.AsV3() + Vector3.up, location.AsV3() + Vector3.up + Vector3.right, color, duration);
        Debug.DrawLine(location.AsV3() + Vector3.right, location.AsV3() + Vector3.up + Vector3.right, color, duration);
        Debug.DrawLine(location.AsV3(), location.AsV3() + Vector3.up + Vector3.right, color, duration);
        Debug.DrawLine(location.AsV3() + Vector3.up, location.AsV3() + Vector3.right, color, duration);
    }


    public static void GizmosDrawTile(Vector2Int location)
    {
        Gizmos.DrawLine(location.AsV3(), location.AsV3() + Vector3.up);
        Gizmos.DrawLine(location.AsV3(), location.AsV3() + Vector3.right);
        Gizmos.DrawLine(location.AsV3() + Vector3.up, location.AsV3() + Vector3.up + Vector3.right);
        Gizmos.DrawLine(location.AsV3() + Vector3.right, location.AsV3() + Vector3.up + Vector3.right);
    }

    public static Vector2 ScreenCenter { get => new Vector2(Screen.width / 2, Screen.height / 2); }

    public static Vector3 MouseToWorld(Camera camera)
    {
        if (camera == null)
            return Vector3.zero;

        var ray = camera.ScreenPointToRay(Input.mousePosition);

        Plane p = new Plane(Vector3.forward, Vector3.zero);
        if (p.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    public static RaycastHit2D[] RaycastFromMouse(Camera camera)
    {
        if (camera == null)
            return null;

        return RaycastFromMouse(camera, int.MaxValue);
    }

    public static RaycastHit2D[] RaycastFromMouse(Camera camera, LayerMask mask)
    {
        if (camera == null)
            return null;

        Vector3 position = MouseToWorld(camera);
        return Physics2D.CircleCastAll(position, 0.2f, Vector2.zero, 1000, mask.value);
    }


    public static Vector2Int AsV2Int(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return new Vector2Int(0, 1);
            case Direction.Right:
                return new Vector2Int(1, 0);
            case Direction.Down:
                return new Vector2Int(0, -1);
            case Direction.Left:
                return new Vector2Int(-1, 0);

            default:
                return Vector2Int.zero;
        }
    }
    public static Direction Inverse(this Direction dir)
    {
        if (dir == Direction.None)
            return Direction.None;

        return (Direction)(((int)dir + 2) % 4);
    }

    public static float AsVerticalFloat(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return 1;
            case Direction.Down:
                return -1;
            default:
                return 0;
        }
    }

    public static string GenerateNewSaveGUID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public static int RandomInV2(Vector2Int vector2Int)
    {
        return Random.Range(vector2Int.x, vector2Int.y + 1);
    }

    public static float RandomInV2(Vector2 vector2)
    {
        return Random.Range(vector2.x, vector2.y);
    }

    public static string MakePathRelative(string p)
    {
        if (p.StartsWith(Application.dataPath))
        {
            p = "Assets" + p.Substring(Application.dataPath.Length);
        }
        return p;
    }

    public static string ChooseRandomString(params string[] stringArray)
    {
        return stringArray[Random.Range(0, stringArray.Length)];
    }

    public static string EnumToString(System.Type type)
    {
        if (type.IsEnum)
        {
            var values = System.Enum.GetNames(type);
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            foreach (var item in values)
            {
                sb.Append(item + ", ");
            }
            sb.Append(")");
            return sb.ToString();
        }
        else
        {
            return "NOT ENUM";
        }
    }

    /// <summary>
    /// Extensive search for T in all GameObjects
    /// </summary>
    public static T[] FindAllThatImplement<T>()
    {
        var objects = GameObject.FindObjectsOfType<GameObject>();
        List<T> interfaces = new List<T>();

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent(out T t))
            {
                interfaces.Add(t);
            }
        }

        return interfaces.ToArray();
    }

    public static bool IsNullOrDestroyed(object t)
    {
        return (t == null || t.Equals(null));
    }

    public static Color AsDebugColor(this CrumbleType t)
    {
        switch (t)
        {
            case CrumbleType.Air:
                return Color.white;
            case CrumbleType.Normal:
                return new Color(1, 0.5f, 0);
            case CrumbleType.Crumble | CrumbleType.Normal:
                return Color.red;
            case CrumbleType.CrumbleInstant | CrumbleType.Normal:
                return Color.magenta;

            case CrumbleType.Unstable:
                return Color.blue;

            case CrumbleType.Anything:
                return Color.yellow;

            default:
                return Color.gray;
        }
    }

    public static bool StringMatches(string text, params string[] options)
    {
        foreach (var o in options)
        {
            if (text == o)
                return true;
        }

        return false;
    }
}
