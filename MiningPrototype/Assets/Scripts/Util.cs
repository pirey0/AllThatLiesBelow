using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
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

    public static  void IterateX(int size, System.Action<int> action)
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
}
