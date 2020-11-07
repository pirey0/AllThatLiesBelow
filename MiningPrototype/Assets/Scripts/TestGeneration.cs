using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class TestGeneration : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase[] groundTiles;

    [Header("Settings")]
    [SerializeField] bool updateOnParameterChanged;

    [OnValueChanged("OnParameterChanged")]
    [SerializeField] bool seedIsRandom;

    [OnValueChanged("OnParameterChanged")]
    [SerializeField] int seed;

    [OnValueChanged("OnParameterChanged")]
    [SerializeField] int size;

    [OnValueChanged("OnParameterChanged")]
    [Range(0, 1)]
    [SerializeField] float initialAliveChance;

    [OnValueChanged("OnParameterChanged")]
    [Range(0, 9)]
    [SerializeField] int deathLimit;

    [OnValueChanged("OnParameterChanged")]
    [Range(0, 9)]
    [SerializeField] int birthLimit;

    [OnValueChanged("OnParameterChanged")]
    [Range(0, 10)]
    [SerializeField] int automataSteps;

    [SerializeField] AnimationCurve heightMultiplyer;

    bool[,] map;

    static readonly Dictionary<int, int> BITMASK_TO_TILEINDEX = new Dictionary<int, int>()
    {{2, 1 },{ 8, 2 }, {10, 3 }, {11, 4 }, {16, 5 }, {18, 6 }, { 22, 7 },
        { 24, 8 }, {26, 9 }, {27, 10 }, {30, 11 }, {31, 12 }, {64, 13 },{ 66 , 14},
        { 72 , 15},{ 74 , 16},{ 75 , 17},{ 80 , 18},{ 82 , 19},{ 86 , 20},{ 88 , 21},
        { 90 , 22},{ 91 , 23},{ 94 , 24},{ 95 , 25},{ 104 , 26},{ 106 , 27},{ 107 , 28},
        { 120 , 29},{ 122 , 30},{ 123 , 31},{ 126 , 32},{ 127 , 33},{ 208 , 34},{ 210 , 35},
        { 214 , 36},{ 216 , 37},{ 218 , 38},{ 219 , 39},{ 222 , 40},{ 223 , 41},{ 248 , 42},
        { 250 , 43},{ 251 , 44},{ 254 , 45},{ 255 , 46},{ 0 , 47 } };

    private void Start()
    {
        RunCompleteGeneration();
    }


    [Button]
    private void RunCompleteGeneration()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Populate();

        IterateX(automataSteps, (x) => RunAutomataStep());

        UpdateVisuals();

        stopwatch.Stop();

        Debug.Log("Update Duration: " + stopwatch.ElapsedMilliseconds + "ms");
    }


    private void Populate()
    {
        if (!seedIsRandom)
            UnityEngine.Random.InitState(seed);

        map = new bool[size, size];

        IterateXY(size, (x, y) => map[x, y] = heightMultiplyer.Evaluate((float)y / size) * UnityEngine.Random.value < initialAliveChance);

    }

    //https://gamedevelopment.tutsplus.com/tutorials/generate-random-cave-levels-using-cellular-automata--gamedev-9664
    private int GetAliveNeightboursCountFor(int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = x + i;
                int neighbour_y = y + j;

                if (i == 0 && j == 0)
                {
                }
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= size || neighbour_y >= size)
                {
                    count = count + 1;
                }
                else if (map[neighbour_x, neighbour_y])
                {
                    count = count + 1;
                }
            }
        }

        return count;
    }

    private bool GetMapAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return false;

        return map[x, y];
    }

    public bool HasLineOfSight(Vector2Int start, Vector2Int end, bool debugVisualize = false)
    {
        Vector2Int current = start;

        while (current != end)
        {
            bool blocked = GetMapAt(current.x, current.y);

            if (blocked)
            {
                if (debugVisualize)
                    Debug.DrawLine((Vector3Int)current, (Vector3Int)end, Color.red, 1);

                return false;
            }

            Vector2Int delta = end - current;
            Vector2Int offset;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                offset = new Vector2Int((int)Mathf.Sign(delta.x), 0);
            else
                offset = new Vector2Int(0, (int)Mathf.Sign(delta.y));

            if (debugVisualize)
                Debug.DrawLine((Vector3Int) current, (Vector3Int) (current + offset), Color.yellow, 1f);

            current += offset;
        }

        return true;
    }



    public void TryCarve(int x, int y)
    {
        Debug.Log("Try Carve " + x + " / " + y);
        SetMapAt(x, y, false);
    }

    public void TryPlace(int x, int y)
    {
        Debug.Log("Try Place " + x + " / " + y);
        SetMapAt(x, y, true);
    }

    private void SetMapAt(int x, int y, bool value, bool updateVisuals = true)
    {
        if (IsOutOfBounds(x, y))
            return;

        map[x, y] = value;

        if (updateVisuals)
        {
            UpdateTileMapAt(x, y);
            UpdateTileMapAt(x + 1, y);
            UpdateTileMapAt(x - 1, y);
            UpdateTileMapAt(x, y + 1);
            UpdateTileMapAt(x, y - 1);
            UpdateTileMapAt(x + 1, y + 1);
            UpdateTileMapAt(x - 1, y - 1);
            UpdateTileMapAt(x - 1, y + 1);
            UpdateTileMapAt(x + 1, y - 1);
        }
    }

    private void RunAutomataStep()
    {
        IterateXY(size, SingleAutomataSet);
    }

    private void SingleAutomataSet(int x, int y)
    {
        int nbs = GetAliveNeightboursCountFor(x, y);
        map[x, y] = map[x, y] ? nbs > deathLimit : nbs > birthLimit;
    }

    void UpdateVisuals()
    {
        tilemap.ClearAllTiles();
        IterateXY(size, UpdateTileMapAt);
    }

    void OnParameterChanged()
    {
        if (updateOnParameterChanged)
        {
            RunCompleteGeneration();
        }
    }

    private void UpdateTileMapAt(int x, int y)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), GetCorrectTile(x, y));
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return (x < 0 || y < 0 || x >= size || y >= size);
    }

    private TileBase GetCorrectTile(int x, int y)
    {

        if (IsOutOfBounds(x, y) || !map[x, y])
            return null;

        int topLeft = GetMapAt(x - 1, y + 1) ? 1 : 0;
        int topMid = GetMapAt(x, y + 1) ? 1 : 0;
        int topRight = GetMapAt(x + 1, y + 1) ? 1 : 0;
        int midLeft = GetMapAt(x - 1, y) ? 1 : 0;
        int midRight = GetMapAt(x + 1, y) ? 1 : 0;
        int botLeft = GetMapAt(x - 1, y - 1) ? 1 : 0;
        int botMid = GetMapAt(x, y - 1) ? 1 : 0;
        int botRight = GetMapAt(x + 1, y - 1) ? 1 : 0;


        int value = topMid * 2 + midLeft * 8 + midRight * 16 + botMid * 64;
        value += topLeft * topMid * midLeft;
        value += topRight * topMid * midRight * 4;
        value += botLeft * midLeft * botMid * 32;
        value += botRight * midRight * botMid * 128;

        int tileIndex = BITMASK_TO_TILEINDEX[value];

        //Casual random tile
        if (tileIndex == 46)
        {
            tileIndex = PseudoRandomValue(x, y) > 0.5f ? 46 : 0;
        }

        return groundTiles[tileIndex];
    }

    private void IterateXY(int size, System.Action<int, int> action)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                action(x, y);
            }
        }
    }

    private void IterateX(int size, System.Action<int> action)
    {
        for (int i = 0; i < size; i++)
        {
            action(i);
        }
    }

    private float PseudoRandomValue(float x, float y)
    {
        return (float)(Mathf.Sin(Vector2.Dot(new Vector2(x, y), new Vector2(12.9898f, 78.233f))) * 43758.5453) % 1;
    }

}


[System.Serializable]
public class CollisionShape
{
    public Vector2[] Points;
}

public enum CollisionShapeType
{
    Square,
    TL_BR_BL,
    TL_TR_BR
}