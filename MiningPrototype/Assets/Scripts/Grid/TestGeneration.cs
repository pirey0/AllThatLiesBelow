using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class TestGeneration : MonoBehaviour
{
    [SerializeField] Tilemap tilemap, damageOverlayTilemap, oreTilemap;
    [SerializeField] TileBase[] groundTiles;
    [SerializeField] TileBase[] damageOverlayTiles;
    [SerializeField] TileBase[] oreTiles;
    [SerializeField] TileBase snowTile1, snowTile2;

    [SerializeField] Inventory playerInventory;

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
    [SerializeField] float goldMaxHeight;
    [SerializeField] float goldVeinProbability;
    [SerializeField] float copperMaxHeight;
    [SerializeField] float copperVeinProbability;

    [SerializeField] int snowStartHeight;

    Tile[,] map;

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

        PopulateOres();

        CalculateNeighboursBitmask();

        PopulateSnow();

        UpdateVisuals();

        stopwatch.Stop();

        Debug.Log("Update Duration: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    private void PopulateSnow()
    {
        IterateXY(size, PopulateSnowAt);
    }

    private void PopulateSnowAt(int x, int y)
    {
        if (y < snowStartHeight)
            return;

        var t = GetTileAt(x, y);


        if (IsBlockAt(x, y) && ((t.NeighbourBitmask & 2) == 0)) 
        {
            t.Type = TileType.Snow;
        }

        SetMapAt(x, y, t, updateNeighbourBitmask: false, updateVisuals: false);
    }

    private void CalculateNeighboursBitmask()
    {
        IterateXY(size, CalculateNeighboursBitmaskAt);
    }

    private void CalculateNeighboursBitmaskAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return;

        int topLeft = IsBlockAt(x - 1, y + 1) ? 1 : 0;
        int topMid = IsBlockAt(x, y + 1) ? 1 : 0;
        int topRight = IsBlockAt(x + 1, y + 1) ? 1 : 0;
        int midLeft = IsBlockAt(x - 1, y) ? 1 : 0;
        int midRight = IsBlockAt(x + 1, y) ? 1 : 0;
        int botLeft = IsBlockAt(x - 1, y - 1) ? 1 : 0;
        int botMid = IsBlockAt(x, y - 1) ? 1 : 0;
        int botRight = IsBlockAt(x + 1, y - 1) ? 1 : 0;

        int value = topMid * 2 + midLeft * 8 + midRight * 16 + botMid * 64;
        value += topLeft * topMid * midLeft;
        value += topRight * topMid * midRight * 4;
        value += botLeft * midLeft * botMid * 32;
        value += botRight * midRight * botMid * 128;

        Tile t = map[x, y];
        t.NeighbourBitmask = (byte)value;
        map[x, y] = t;

    }

    private void Populate()
    {
        if (!seedIsRandom)
            UnityEngine.Random.InitState(seed);

        map = new Tile[size, size];

        IterateXY(size, PopulateAt);

    }

    private void PopulateOres()
    {
        IterateX((int)(size * size * goldVeinProbability), TryPlaceGoldVein);
        IterateX((int)(size * size * copperVeinProbability), TryPlaceCopperVein);

    }

    private void TryPlaceCopperVein(int obj)
    {
        int y = UnityEngine.Random.Range(0, (int)(copperMaxHeight * size));
        int x = UnityEngine.Random.Range(0, size);

        if (IsBlockAt(x, y))
        {
            SetMapAt(x, y, Tile.Make(TileType.Copper), updateNeighbourBitmask: false, updateVisuals: false);
            Debug.Assert(GetTileAt(x, y).Type == TileType.Copper);
        }
    }

    private void TryPlaceGoldVein(int obj)
    {
        int y = UnityEngine.Random.Range(0, (int)(goldMaxHeight*size));
        int x = UnityEngine.Random.Range(0, size);

        if (IsBlockAt(x, y))
        {
            SetMapAt(x, y, Tile.Make(TileType.Gold), updateNeighbourBitmask: false, updateVisuals: false);
        }
    }

    private void PopulateAt(int x, int y)
    {
        Tile t = Tile.Air;

        bool occupied = heightMultiplyer.Evaluate((float)y / size) * UnityEngine.Random.value < initialAliveChance;

        if (occupied)
            t.Type = TileType.Stone;

        map[x, y] = t;
    }

    //https://gamedevelopment.tutsplus.com/tutorials/generate-random-cave-levels-using-cellular-automata--gamedev-9664
    private int GetAliveNeightboursCountFor(int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int nx = x + i;
                int ny = y + j;

                if (i == 0 && j == 0)
                {
                }
                else if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                {
                    count = count + 1;
                }
                else if (IsBlockAt(nx, ny))
                {
                    count = count + 1;
                }
            }
        }

        return count;
    }

    public bool IsAirAt(int x, int y)
    {
        return GetTileAt(x, y).Type == TileType.Air;
    }

    public bool IsBlockAt(int x, int y)
    {
        return GetTileAt(x, y).Type != TileType.Air;
    }

    public Tile GetTileAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return Tile.Air;

        return map[x, y];
    }

    public bool HasLineOfSight(Vector2Int start, Vector2Int end, bool debugVisualize = false)
    {
        Vector2Int current = start;

        while (current != end)
        {
            bool blocked = IsBlockAt(current.x, current.y);

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

    public Vector3 GetWorldLocationOfFreeFaceFromSource(Vector2Int target, Vector2Int source)
    {
        Vector2Int disp = source - target;

        if (Mathf.Abs(disp.x) > Mathf.Abs(disp.y))
        {
            bool xAir = IsAirAt(target.x + (int)Mathf.Sign(disp.x), target.y);
            if (xAir)
                return (Vector3Int)target + new Vector3((int)Mathf.Sign(disp.x) * 0.5f + 0.5f, 0.5f, 0);
            else
                return (Vector3Int)target + new Vector3(0.5f, (int)Mathf.Sign(disp.y) * 0.5f + 0.5f, 0);
        }
        else
        {
            bool yAir = IsAirAt(target.x, target.y + (int)Mathf.Sign(disp.y));
            if (yAir)
                return (Vector3Int)target + new Vector3(0.5f, (int)Mathf.Sign(disp.y) * 0.5f + 0.5f, 0);
            else
                return (Vector3Int)target + new Vector3((int)Mathf.Sign(disp.x) * 0.50f + 0.5f, 0.5f, 0);
        }

    }

    public Vector2Int GetClosestSolidBlock(Vector2Int current, Vector2Int end)
    {
        while (current != end)
        {
            if (IsBlockAt(current.x, current.y))
                return current;

            current += StepTowards(current, end);
        }
        return end;
    }

    public bool DamageAt(int x, int y, float amount)
    {
        if (IsOutOfBounds(x, y))
            return false;

        Tile t = GetTileAt(x, y);
        t.TakeDamage(amount);

        //
        if (t.Damage > 10)
        {
            BreakBlock(x, y, t);

            return true;
        }
        else
        {
            SetMapAt(x, y, t, updateNeighbourBitmask: false, updateVisuals: true);
            return false;
        }
    }

    private void BreakBlock(int x, int y, Tile t)
    {
        CarveAt(x, y);

        itemType itemType = itemType.ROCKS;

        switch (t.Type)
        {
            case TileType.Gold:
                itemType = itemType.GOLD;
                break;

            case TileType.Copper:
                itemType = itemType.COPPER;
                break;
        }

        playerInventory.Add(itemType, 1);
    }

    public void CarveAt(int x, int y)
    {
        Debug.Log("Try Carve " + x + " / " + y);
        SetMapAt(x, y, Tile.Air);
    }

    public void PlaceAt(int x, int y)
    {
        Debug.Log("Try Place " + x + " / " + y);
        SetMapAt(x, y, Tile.Stone);
    }

    private void SetMapAt(int x, int y, Tile value, bool updateNeighbourBitmask = true, bool updateVisuals = true)
    {
        if (IsOutOfBounds(x, y))
            return;

        map[x, y] = value;

        if (updateNeighbourBitmask)
        {
            CalculateNeighboursBitmaskAt(x, y);
            foreach (var nIndex in GetNeighboursIndiciesOf(x, y))
            {
                CalculateNeighboursBitmaskAt(nIndex.x, nIndex.y);
            }
        }

        if (updateVisuals)
        {
            UpdateVisualsAt(x, y);
            foreach (var nIndex in GetNeighboursIndiciesOf(x, y))
            {
                UpdateVisualsAt(nIndex.x, nIndex.y);
            }
        }
    }

    private Vector2Int[] GetNeighboursIndiciesOf(int x, int y)
    {
        return new Vector2Int[]
        {
            new Vector2Int(x+1,y),
            new Vector2Int(x,y+1),
            new Vector2Int(x+1,y+1),
            new Vector2Int(x-1,y),
            new Vector2Int(x,y-1),
            new Vector2Int(x-1,y-1),
            new Vector2Int(x+1,y-1),
            new Vector2Int(x-1,y+1)
        };
    }

    private void RunAutomataStep()
    {
        IterateXY(size, SingleAutomataSet);
    }

    private void SingleAutomataSet(int x, int y)
    {
        int nbs = GetAliveNeightboursCountFor(x, y);
        map[x, y] = IsBlockAt(x, y) ? (nbs > deathLimit ? Tile.Stone : Tile.Air) : (nbs > birthLimit ? Tile.Stone : Tile.Air);
    }

    void UpdateVisuals()
    {
        tilemap.ClearAllTiles();
        damageOverlayTilemap.ClearAllTiles();
        oreTilemap.ClearAllTiles();
        IterateXY(size, UpdateVisualsAt);
    }

    void OnParameterChanged()
    {
        if (updateOnParameterChanged)
        {
            RunCompleteGeneration();
        }
    }

    private void UpdateVisualsAt(int x, int y)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), GetVisualTileFor(x, y));
        damageOverlayTilemap.SetTile(new Vector3Int(x, y, 0), GetVisualDestructableOverlayFor(x, y));
        oreTilemap.SetTile(new Vector3Int(x, y, 0), GetVisualOreTileFor(x, y));
        
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return (x < 0 || y < 0 || x >= size || y >= size);
    }

    private TileBase GetVisualTileFor(int x, int y)
    {
        Tile tile = GetTileAt(x, y);

        if (IsOutOfBounds(x, y) || IsAirAt(x, y))
            return null;

        if (tile.Type == TileType.Snow)
        {
            return PseudoRandomValue(x, y) > 0.5f ? snowTile1 : snowTile2;
        }

        int tileIndex = BITMASK_TO_TILEINDEX[tile.NeighbourBitmask];

        //Casual random tile
        if (tileIndex == 46)
        {
            tileIndex = PseudoRandomValue(x, y) > 0.5f ? 46 : 0;
        }

        return groundTiles[tileIndex];
    }

    private TileBase GetVisualDestructableOverlayFor(int x, int y)
    {
        var t = GetTileAt(x, y);
        return damageOverlayTiles[Mathf.FloorToInt(t.Damage)];
    }

    private TileBase GetVisualOreTileFor(int x, int y)
    {
        var t = GetTileAt(x, y);
        if ((int)t.Type < 2 || (int)t.Type == 4)
            return null;

        return oreTiles[(int)t.Type - 2];
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
