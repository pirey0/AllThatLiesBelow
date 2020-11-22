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
public class RuntimeProceduralMap : RenderedMap
{
    private static RuntimeProceduralMap instance;

    [Header("RuntimeProceduralMap")]
    [SerializeField] Transform entitiesParent;


    [SerializeField] bool debug;

    ITileUpdateReceiver[,] receiverMap;
    List<Vector2Int> unstableTiles = new List<Vector2Int>();
    List<GameObject> unstableTilesEffects = new List<GameObject>();
    Stack<Vector2Int> tilesToStabilityCheck = new Stack<Vector2Int>();

    public event System.Action<MirrorState> MirrorSideChanged;

    public static RuntimeProceduralMap Instance { get => instance; }


    protected virtual void Awake()
    {

        if (Instance != this && instance != null)
        {
            Destroy(gameObject);
            Debug.LogError("Second RuntimeProceduralMap found.");
            return;
        }
        else
        {
            instance = this;
        }

        Setup();

        RunCompleteGeneration();
    }
    protected override void Setup()
    {
        base.Setup();

        if (MapSettings == null || GenerationSettings == null)
        {
            Debug.LogError("TileMap settings missing.");
            return;
        }
        receiverMap = new ITileUpdateReceiver[SizeX, SizeY];
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        receiverMap = null;
        unstableTiles = null;
        unstableTilesEffects = null;
        tilesToStabilityCheck = null;
    }

    protected override void OnStateChanged(GameState.State newState)
    {
        if (newState == GameState.State.Ready)
        {
            StartCoroutine(UpdateUnstableTilesRoutine());
        }
    }

    private void Update()
    {
        if (debug)
        {
            TooltipHandler.Instance?.Display(transform, this[Util.MouseToWorld().ToGridPosition()].ToString(), "");
        }
    }

    private IEnumerator UpdateUnstableTilesRoutine()
    {
        tilesToStabilityCheck.Clear(); //clear on real start

        int i = 0;
        while (true)
        {
            //loop through tiles to check
            while (tilesToStabilityCheck.Count > 0)
            {
                var loc = tilesToStabilityCheck.Pop();
                var tile = this[loc];
                var info = GetTileInfo(tile.Type);
                if (info.StabilityAffected)
                {
                    if (tile.Stability <= GenerationSettings.UnstableThreshhold)
                    {
                        float timeLeft = tile.Stability - GenerationSettings.CollapseThreshhold;
                        AddUnstableTile(loc, timeLeft);
                    }
                }
            }

            //iterate through unstable tiles
            if (unstableTiles.Count == 0)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }
            else if (i >= unstableTiles.Count)
            {
                yield return new WaitForSeconds(1f);
                i = 0;
            }

            var t = this[unstableTiles[i]];
            int x = unstableTiles[i].x;
            int y = unstableTiles[i].y;

            if (t.Stability <= GenerationSettings.CollapseThreshhold)
            {
                RemoveUnstableTileAt(i);
                var info = GetTileInfo(t.Type);
                if (info.NotifiesInsteadOfCrumbling)
                {
                    receiverMap[x, y]?.OnTileCrumbleNotified(x, y);
                }
                else
                {
                    if (t.Type != TileType.Air)
                        CollapseAt(x, y, updateVisuals: true);
                }
            }
            else if (t.Stability <= GenerationSettings.UnstableThreshhold)
            {
                t.ReduceStabilityBy(1);

                SetMapRawAt(x, y, t);
                i++;
            }
            else
            {
                RemoveUnstableTileAt(i);
            }
        }
    }

    public GameObject InstantiateEntity(GameObject prefab, Vector3 position)
    {
        var go = GameObject.Instantiate(prefab, entitiesParent);
        go.transform.localPosition = position;

        return go;
    }

    public void NotifyMirrorWorldSideChange(MirrorState newState)
    {
        MirrorSideChanged?.Invoke(newState);
    }

    public enum MirrorState { Center, Right, Left };


    public void AddTileToCheckForStability(Vector2Int tileLoc)
    {
        tilesToStabilityCheck.Push(tileLoc);
    }

    private void AddUnstableTile(Vector2Int unstableTile, float timeLeftToCrumble)
    {
        if (!unstableTiles.Contains(unstableTile))
        {

            unstableTiles.Add(unstableTile);
            var go = Instantiate(MapSettings.CrumbleEffects, new Vector3(unstableTile.x + 0.5f, unstableTile.y), quaternion.identity, transform);
            go.GetComponent<CrumblingParticle>().SetDuration(timeLeftToCrumble);

            unstableTilesEffects.Add(go);
        }

    }

    public void RemoveUnstableTileAt(int i)
    {
        unstableTiles.RemoveAt(i);

        var go = unstableTilesEffects[i];
        Destroy(go);
        unstableTilesEffects.RemoveAt(i);
    }

    public void SetReceiverMapAt(int x, int y, ITileUpdateReceiver receiver)
    {
        if (IsOutOfBounds(x, y))
            return;

        receiverMap[x, y] = receiver;
    }

    protected override void UpdatePropertiesAt(int x, int y, Tile newTile, Tile previousTile, TileUpdateReason reason)
    {
        CalculateNeighboursBitmaskAt(x, y);

        foreach (var nIndex in MapHelper.GetNeighboursIndiciesOf(x, y))
        {
            CalculateNeighboursBitmaskAt(nIndex.x, nIndex.y);
        }

        PropagateStabilityUpdatesFrom(x, y);

        if (previousTile.Type != newTile.Type)
        {
            receiverMap[x, y]?.OnTileUpdated(x, y, reason);
            receiverMap[x, y] = null;
        }
    }

    private void CalculateNeighboursBitmaskAt(int x, int y)
    {
        if (IsOutOfBounds(x, y))
            return;

        int topLeft = IsNeighbourAt(x - 1, y + 1) ? 1 : 0;
        int topMid = IsNeighbourAt(x, y + 1) ? 1 : 0;
        int topRight = IsNeighbourAt(x + 1, y + 1) ? 1 : 0;
        int midLeft = IsNeighbourAt(x - 1, y) ? 1 : 0;
        int midRight = IsNeighbourAt(x + 1, y) ? 1 : 0;
        int botLeft = IsNeighbourAt(x - 1, y - 1) ? 1 : 0;
        int botMid = IsNeighbourAt(x, y - 1) ? 1 : 0;
        int botRight = IsNeighbourAt(x + 1, y - 1) ? 1 : 0;

        int value = topMid * 2 + midLeft * 8 + midRight * 16 + botMid * 64;
        value += topLeft * topMid * midLeft;
        value += topRight * topMid * midRight * 4;
        value += botLeft * midLeft * botMid * 32;
        value += botRight * midRight * botMid * 128;

        Tile t = this[x, y];
        t.NeighbourBitmask = (byte)value;
        this[x, y] = t;

    }

    private void PropagateStabilityUpdatesFrom(int x, int y)
    {
        MarkToCheckForStability(x, y);
        ResetStability(x, y);
        DirectionalStabilityIterator(x, y, Direction.Up);
        DirectionalStabilityIterator(x, y, Direction.Right);
        DirectionalStabilityIterator(x, y, Direction.Down);
        DirectionalStabilityIterator(x, y, Direction.Left);
    }

    private void DirectionalStabilityIterator(int x, int y, Direction dir)
    {
        Vector2Int offset = dir.Inverse().AsV2Int();

        for (int i = 0; i < GenerationSettings.StabilityPropagationDistance; i++)
        {
            SetDirectionalStabilityAt(x, y, dir);
            MarkToCheckForStability(x, y);
            x += offset.x;
            y += offset.y;
        }
    }

    private void ResetStability(int x, int y)
    {
        var tile = GetTileAt(x, y);
        tile.ResetStability();

        this[x, y] = tile;
    }

    private void SetDirectionalStabilityAt(int x, int y, Direction direction)
    {
        if (IsAirAt(x, y))
            return;

        var tile = GetTileAt(x, y);

        if (direction == Direction.Down)
            tile.SetStability(direction, IsBlockAt(x, y - 1) ? 100 : 0);
        else
        {
            Vector2Int offset = direction.AsV2Int();
            tile.SetStability(direction, Mathf.Min(25, this[x + offset.x, y + offset.y].Stability / 4));
        }

        this[x, y] = tile;
    }

    private void MarkToCheckForStability(int x, int y)
    {
        AddTileToCheckForStability(new Vector2Int(x, y));
    }



    public void RunCompleteGeneration()
    {
        DurationTracker tracker = new DurationTracker("Complete generation");

        Populate();

        Util.IterateX(GenerationSettings.AutomataSteps, (x) => RunAutomataStep());

        PopulateOres();

        PupulateRocks();

        PopulateBorders();

        CalculateNeighboursBitmask();

        CalculateStabilityAll();

        PopulateSnow();

        UpdateAllVisuals();

        tracker.Stop();
    }

    private void PopulateBorders()
    {
        Util.IterateX(SizeX, (x) => this[x, 0] = Tile.Make(TileType.BedStone));
    }

    private void PupulateRocks()
    {
        foreach (var pass in GenerationSettings.RockPasses)
        {
            for (int y = 0; y < this.SizeY; y++)
            {
                Util.IterateX((int)(this.SizeX * pass.Probability.Evaluate((float)y / this.SizeY) * 0.01f), (x) => TryPlaceRock(pass, y));
            }
        }
    }

    private void TryPlaceRock(RockPass pass, int y)
    {
        int x = UnityEngine.Random.Range(0, SizeX);

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


        if (MapHelper.IsAllBlockAt(this, spawnCheckLocations.ToArray()))
        {
            foreach (var loc in locations)
            {
                SetMapAt(loc.x, loc.y, Tile.Air, TileUpdateReason.Generation, updateProperties: false, updateVisuals: false);
            }

            Vector3 pos = new Vector3(x + pass.Size.x * 0.5f, y + pass.Size.y * 0.5f);
            var go = InstantiateEntity(pass.Prefab, pos);
            go.GetComponent<ITileMapElement>().Setup(this);
        }
    }


    private bool ShouldCollapseAt(int x, int y)
    {
        return IsBlockAt(x, y) && GetTileAt(x, y).Stability <= GenerationSettings.CollapseThreshhold;
    }

    public void CollapseAt(int x, int y, bool updateVisuals)
    {
        Tile t = map[x, y];
        SetMapAt(x, y, Tile.Air, TileUpdateReason.Collapse, updateProperties: true, updateVisuals);

        var go = GameObject.Instantiate(GenerationSettings.PhysicalTilePrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
        go.GetComponent<PhysicalTile>().Setup(this, t, GetTileInfo(t.Type));
    }

    private void CalculateStabilityAll()
    {
        Util.IterateXY(SizeX, SizeY, (x, y) => ResetStability(x, y));

        Util.IterateXY(SizeX, SizeY, (x, y) => SetDirectionalStabilityAt(x, y, Direction.Down));

        for (int y = SizeY; y >= 0; y--)
        {
            for (int x = 0; x < SizeX; x++)
            {
                SetDirectionalStabilityAt(x, y, Direction.Up);
            }
        }

        for (int y = 0; y < SizeY; y++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                SetDirectionalStabilityAt(x, y, Direction.Left);
            }

            for (int x = SizeX; x >= 0; x--)
            {
                SetDirectionalStabilityAt(x, y, Direction.Right);
            }
        }
    }



    private void PopulateSnow()
    {
        Util.IterateXY(SizeX, SizeY, PopulateSnowAt);
    }

    private void PopulateSnowAt(int x, int y)
    {
        if (y < GenerationSettings.SnowStartHeight)
            return;

        var t = GetTileAt(x, y);


        if (IsBlockAt(x, y) && ((t.NeighbourBitmask & 2) == 0))
        {
            t.Type = TileType.Snow;
        }

        SetMapAt(x, y, t, TileUpdateReason.Generation, updateProperties: false, updateVisuals: false);
    }

    private void CalculateNeighboursBitmask()
    {
        Util.IterateXY(SizeX, SizeY, CalculateNeighboursBitmaskAt);
    }


    private void Populate()
    {
        if (!GenerationSettings.SeedIsRandom)
            UnityEngine.Random.InitState(GenerationSettings.Seed);

        InitMap(SizeX, SizeY);

        Util.IterateXY(SizeX, SizeY, PopulateAt);

    }

    private void PopulateOres()
    {
        foreach (var pass in GenerationSettings.OrePasses)
        {
            for (int y = 0; y < SizeY; y++)
            {
                Util.IterateX((int)(SizeX * pass.Probability.Evaluate((float)y / SizeY) * 0.01f), (x) => TryPlaceVein(pass.TileType, Util.RandomInVector(pass.OreVeinSize), y));
            }
        }
    }

    private void TryPlaceVein(TileType type, int amount, int y)
    {

        int x = UnityEngine.Random.Range(0, SizeX);

        GrowVeinAt(x, y, type, amount);
    }

    private void GrowVeinAt(int startX, int startY, TileType tile, int amount)
    {
        int x = startX;
        int y = startY;
        int attemptsLeft = amount * 10;

        while (amount > 0 && attemptsLeft > 0)
        {
            if (IsBlockAt(x, y))
            {
                if (GetTileAt(x, y).Type != tile)
                {
                    SetMapAt(x, y, Tile.Make(tile), TileUpdateReason.Generation, updateProperties: false, updateVisuals: false);
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

        bool occupied = GenerationSettings.HeightMultiplyer.Evaluate((float)y / SizeY) * UnityEngine.Random.value < GenerationSettings.InitialAliveCurve.Evaluate((float)y / SizeY);

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
                else if (nx < 0 || ny < 0 || nx >= SizeX || ny >= SizeY)
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

    private void RunAutomataStep()
    {
        Util.IterateXY(SizeX, SizeY, SingleAutomataSet);
    }

    private void SingleAutomataSet(int x, int y)
    {
        int nbs = GetAliveNeightboursCountFor(x, y);
        map[x, y] = IsBlockAt(x, y) ? (nbs > GenerationSettings.DeathLimit ? Tile.Make(TileType.Stone) : Tile.Air) : (nbs > GenerationSettings.BirthLimit ? Tile.Make(TileType.Stone) : Tile.Air);
    }
}
