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

    [Zenject.Inject] TooltipHandler tooltipHandler;
    [Zenject.Inject] CameraController cameraController;

    [Zenject.Inject] ProgressionHandler progressionHandler;
    [Zenject.Inject] Zenject.DiContainer diContainer;

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
            tooltipHandler?.Display(transform, this[Util.MouseToWorld(cameraController.Camera).ToGridPosition()].ToString(), "");
        }
    }

    private IEnumerator UpdateUnstableTilesRoutine()
    {
        tilesToStabilityCheck.Clear(); //clear on real start

        int i = 0;
        while (true)
        {
            int collapseThreshold = progressionHandler.InstableWorld ? GenerationSettings.instableWorldCollapseThreshhold : GenerationSettings.CollapseThreshhold;
            int unstableThreshhold = progressionHandler.InstableWorld ? GenerationSettings.instableWorldUnstableThreshhold : GenerationSettings.UnstableThreshhold;
            int speed = progressionHandler.InstableWorld ? 3 : 1;

            //loop through tiles to check
            while (tilesToStabilityCheck.Count > 0)
            {
                var loc = tilesToStabilityCheck.Pop();
                var tile = this[loc];
                var info = GetTileInfo(tile.Type);
                if (info.StabilityAffected)
                {
                    if (tile.Stability <= unstableThreshhold)
                    {
                        float timeLeft = tile.Stability - GenerationSettings.CollapseThreshhold; // to adapt to unstable world
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

            if (t.Stability <= collapseThreshold)
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
            else if (t.Stability <= unstableThreshhold)
            {
                t.ReduceStabilityBy(speed);

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
        if (diContainer != null)
            diContainer.InjectGameObject(go);
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
            diContainer.InjectGameObject(go);
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

    protected override void MarkToCheckForStability(int x, int y)
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

        CalculateAllNeighboursBitmask();

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
        WrapXIfNecessary(ref x);
        Tile t = map[x, y];
        SetMapAt(x, y, Tile.Air, TileUpdateReason.Collapse, updateProperties: true, updateVisuals);

        var go = GameObject.Instantiate(GenerationSettings.PhysicalTilePrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
        diContainer.InjectGameObject(go);
        go.GetComponent<PhysicalTile>().Setup(this, t, GetTileInfo(t.Type));
    }

    protected override void UpdatePropertiesAt(int x, int y, Tile newTile, Tile previousTile, TileUpdateReason reason)
    {
        base.UpdatePropertiesAt(x, y, newTile, previousTile, reason);

        if (previousTile.Type != newTile.Type)
        {
            receiverMap[x, y]?.OnTileUpdated(x, y, reason);
            receiverMap[x, y] = null;
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
        if (y == 0)
            occupied = true;

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
                    continue;
                }
                else if (ny < 0 || ny >= SizeY)
                {
                    continue;
                }

                if (nx < 0 || nx >= SizeX)
                {
                    WrapXIfNecessary(ref x);
                }

                if (IsBlockAt(nx, ny))
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
