using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMapGenerator
{
    private TileMap map;
    private GenerationSettings settings;

    public TileMapGenerator(TileMap tileMap, GenerationSettings settings)
    {
        this.map = tileMap;
        this.settings = settings;
    }

    public void RunCompleteGeneration()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        ClearAllEntities();

        Populate();

        Util.IterateX(settings.AutomataSteps, (x) => RunAutomataStep());

        PopulateOres();

        PupulateRocks();

        PopulateBorders();

        CalculateNeighboursBitmask();

        CalculateStabilityAll();

        PopulateSnow();

        stopwatch.Stop();

        Debug.Log("Update Duration: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    private void PopulateBorders()
    {
        Util.IterateX(settings.Size, (x) => map[x, 0] = Tile.Make(TileType.BedStone));
        Util.IterateX(settings.Size, (x) => map[0, x] = Tile.Make(TileType.BedStone));
        Util.IterateX(settings.Size, (x) => map[settings.Size - 1, x] = Tile.Make(TileType.BedStone));
    }

    private void ClearAllEntities()
    {
        for (int i = map.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(map.transform.GetChild(i).gameObject);
        }
    }

    private void PupulateRocks()
    {
        foreach (var pass in settings.RockPasses)
        {
            Util.IterateX((int)(pass.MaxHeight * settings.Size * pass.Probability * 0.01f), (x) => TryPlaceRock(pass));
        }
    }

    private void TryPlaceRock(RockPass pass)
    {
        int y = UnityEngine.Random.Range(0, pass.MaxHeight);
        int x = UnityEngine.Random.Range(0, settings.Size);

        List<Vector2Int> locations = new List<Vector2Int>();
        List<Vector2Int> spawnCheckLocations = new List<Vector2Int>();

        for (int px = 0; px < pass.Size.x; px++)
        {
            for (int py = 0; py < pass.Size.y; py++)
            {
                locations.Add(new Vector2Int(x + px, y + py));
            }
        }

        for (int px = -1; px < pass.Size.x + 1; px++)
        {
            for (int py = -1; py < pass.Size.y + 1; py++)
            {
                spawnCheckLocations.Add(new Vector2Int(x + px, y + py));
            }
        }


        if (TileMapHelper.IsAllBlockAt(map, spawnCheckLocations.ToArray()))
        {
            foreach (var loc in locations)
            {
                map.SetMapAt(loc.x, loc.y, Tile.Air, updateProperties: false, updateVisuals: false);
            }

            Vector3 pos = new Vector3(x + pass.Size.x * 0.5f, y + pass.Size.y * 0.5f);
            var go = GameObject.Instantiate(pass.Prefab, pos, Quaternion.identity, map.transform);
            go.GetComponent<GridElement>().Setup(map);
        }
    }

    public void UpdatePropertiesAt(int x, int y)
    {
        CalculateNeighboursBitmaskAt(x, y);

        foreach (var nIndex in TileMapHelper.GetNeighboursIndiciesOf(x, y))
        {
           CalculateNeighboursBitmaskAt(nIndex.x, nIndex.y);
        }

        PropagateStabilityUpdatesFrom(x, y);
    }
    private bool ShouldCollapseAt(int x, int y)
    {
        return map.IsBlockAt(x, y) && map.GetTileAt(x, y).Stability <= settings.CollapseThreshhold;
    }

    public void CollapseAt(int x, int y, bool updateVisuals)
    {
        Tile t = map[x, y];
        map.SetMapAt(x, y, Tile.Air, updateProperties: true, updateVisuals);
        var go = GameObject.Instantiate(settings.PhysicalTilePrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
        go.GetComponent<PhysicalTile>().Setup(map, t, map.GetTileInfo(t.Type));
    }

    private void CalculateStabilityAll()
    {
        Util.IterateXY(settings.Size, (x, y) => CalculateBaseStability(x, y));

        for (int y = settings.Size; y >= 0; y--)
        {
            for (int x = 0; x < settings.Size; x++)
            {
                SetDirectionalStabilityAt(x, y, Direction.Up);
            }
        }

        for (int y = 0; y < settings.Size; y++)
        {
            for (int x = 0; x < settings.Size; x++)
            {
                SetDirectionalStabilityAt(x, y, Direction.Left);
            }

            for (int x = settings.Size; x >= 0; x--)
            {
                SetDirectionalStabilityAt(x, y, Direction.Right);
            }
        }

    }

    //To change
    private void PropagateStabilityUpdatesFrom(int x, int y)
    {
        CalculateBaseStability(x, y);
        MarkToCheckForStability(x, y);
        DirectionalStabilityIterator(x, y - 1, Direction.Up);
        DirectionalStabilityIterator(x - 1, y, Direction.Right);
        DirectionalStabilityIterator(x, y + 1, Direction.Down);
        DirectionalStabilityIterator(x + 1, y, Direction.Left);
    }

    private void DirectionalStabilityIterator(int x, int y, Direction dir)
    {
        Vector2Int offset = dir.Inverse().AsV2Int();

        do
        {
            SetDirectionalStabilityAt(x, y, dir);
            MarkToCheckForStability(x, y);
            x += offset.x;
            y += offset.y;
        }
        while (!map[x, y].IsStable());
    }

    [System.Obsolete("Old Temporary System")]
    private void ApproximateStabilityFor(int x, int y)
    {
        CalculateBaseStability(x, y);
        SetDirectionalStabilityAt(x, y, Direction.Up);
        SetDirectionalStabilityAt(x, y, Direction.Right);
        SetDirectionalStabilityAt(x, y, Direction.Left);
        MarkToCheckForStability(x, y);
    }

    private void CalculateBaseStability(int x, int y)
    {
        if (map.IsAirAt(x, y))
            return;

        var tile = map.GetTileAt(x, y);
        tile.ResetStability();

        if ((tile.NeighbourBitmask & 64) == 64)
            tile.SetStability(Direction.Down, 100);

        map[x, y] = tile;
    }

    private void SetDirectionalStabilityAt(int x, int y, Direction direction)
    {
        if (map.IsAirAt(x, y))
            return;

        var tile = map.GetTileAt(x, y);
        int divisor = direction == Direction.Down ? 1 : 4;

        Vector2Int offset = direction.AsV2Int();
        tile.SetStability(direction, map[x + offset.x, y + offset.y].Stabilities[(int)direction] / divisor);
        map[x, y] = tile;
    }

    private void MarkToCheckForStability(int x, int y)
    {
        map.AddTileToCheckForStability(new Vector2Int(x, y));
    }

    private void PopulateSnow()
    {
        Util.IterateXY(settings.Size, PopulateSnowAt);
    }

    private void PopulateSnowAt(int x, int y)
    {
        if (y < settings.SnowStartHeight)
            return;

        var t = map.GetTileAt(x, y);


        if (map.IsBlockAt(x, y) && ((t.NeighbourBitmask & 2) == 0))
        {
            t.Type = TileType.Snow;
        }

        map.SetMapAt(x, y, t, updateProperties: false, updateVisuals: false);
    }

    private void CalculateNeighboursBitmask()
    {
        Util.IterateXY(settings.Size, CalculateNeighboursBitmaskAt);
    }

    private void CalculateNeighboursBitmaskAt(int x, int y)
    {
        if (map.IsOutOfBounds(x, y))
            return;

        int topLeft = map.IsBlockAt(x - 1, y + 1) ? 1 : 0;
        int topMid = map.IsBlockAt(x, y + 1) ? 1 : 0;
        int topRight = map.IsBlockAt(x + 1, y + 1) ? 1 : 0;
        int midLeft = map.IsBlockAt(x - 1, y) ? 1 : 0;
        int midRight = map.IsBlockAt(x + 1, y) ? 1 : 0;
        int botLeft = map.IsBlockAt(x - 1, y - 1) ? 1 : 0;
        int botMid = map.IsBlockAt(x, y - 1) ? 1 : 0;
        int botRight = map.IsBlockAt(x + 1, y - 1) ? 1 : 0;

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
        if (!settings.SeedIsRandom)
            UnityEngine.Random.InitState(settings.Seed);

        map.InitMap(settings.Size, settings.Size);

        Util.IterateXY(settings.Size, PopulateAt);

    }

    private void PopulateOres()
    {
        foreach (var pass in settings.OrePasses)
        {
            Util.IterateX((int)(pass.MaxHeight * settings.Size * pass.Probability * 0.01f), (x) => TryPlaceVein(pass.TileType, Util.RandomInVector(pass.OreVeinSize), pass.MaxHeight));
        }
    }

    private void TryPlaceVein(TileType type, int amount, int maxHeight)
    {
        int y = UnityEngine.Random.Range(0, maxHeight);
        int x = UnityEngine.Random.Range(0, settings.Size);

        GrowVeinAt(x, y, type, amount);
    }

    private void GrowVeinAt(int startX, int startY, TileType tile, int amount)
    {
        int x = startX;
        int y = startY;
        int attemptsLeft = amount * 10;

        while (amount > 0 && attemptsLeft > 0)
        {
            if (map.IsBlockAt(x, y))
            {
                if (map.GetTileAt(x, y).Type != tile)
                {
                    map.SetMapAt(x, y, Tile.Make(tile), updateProperties: false, updateVisuals: false);
                    amount--;
                    x = startX;
                    y = startY;
                }
                else
                {
                    var dir = Util.RandomDirection();
                    x += dir.x;
                    y += dir.y;
                }
            }
            else
            {
                x = startX;
                y = startY;
            }
            attemptsLeft--;
        }
    }

    private void PopulateAt(int x, int y)
    {
        Tile t = Tile.Air;

        bool occupied = settings.HeightMultiplyer.Evaluate((float)y / settings.Size) * UnityEngine.Random.value < settings.InitialAliveChance;

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
                else if (nx < 0 || ny < 0 || nx >= settings.Size || ny >= settings.Size)
                {
                    count = count + 1;
                }
                else if (map.IsBlockAt(nx, ny))
                {
                    count = count + 1;
                }
            }
        }

        return count;
    }

    private void RunAutomataStep()
    {
        Util.IterateXY(settings.Size, SingleAutomataSet);
    }

    private void SingleAutomataSet(int x, int y)
    {
        int nbs = GetAliveNeightboursCountFor(x, y);
        map[x, y] = map.IsBlockAt(x, y) ? (nbs > settings.DeathLimit ? Tile.Make(TileType.Stone) : Tile.Air) : (nbs > settings.BirthLimit ? Tile.Make(TileType.Stone) : Tile.Air);
    }

}
