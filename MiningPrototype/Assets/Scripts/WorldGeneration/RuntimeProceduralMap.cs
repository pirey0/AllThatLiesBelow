using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.Tilemaps;

public class UnstableTile
{
    public Vector2Int Location;
    public GameObject Effects;
    public float Duration;

    private float spawnStamp;

    public UnstableTile()
    {
        spawnStamp = Time.time;
    }

    internal bool ShouldCollapse()
    {
        return Time.time - spawnStamp > Duration;
    }
}

[DefaultExecutionOrder(-100)]
public class RuntimeProceduralMap : RenderedMap
{
    [Header("RuntimeProceduralMap")]
    [ReadOnly]
    [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] Transform entitiesParent;
    [SerializeField] Material unlitMaterial, litMaterial;

    [Zenject.Inject] ProgressionHandler progressionHandler;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] PrefabFactory prefabFactory;
    [Zenject.Inject] SaveHandler saveHandler;
    [Zenject.Inject] SceneAdder sceneAdder;

    bool[,] additiveCoveredMap;
    ITileUpdateReceiver[,] receiverMap;
    List<UnstableTile> unstableTiles = new List<UnstableTile>();

    public event System.Action<MirrorState> MirrorSideChanged;
    public event System.Action<TileType> MinedBlock;

    protected virtual void Awake()
    {
        Setup();
    }

    protected override void OnStateChanged(GameState.State newState)
    {
        if (newState == GameState.State.PreLoadScenes)
        {
            StartCoroutine(RunCompleteGeneration());
        }
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
    }

    protected override void OnRealStart()
    {
        RefreshAll();
        StartCoroutine(UpdateUnstableTilesRoutine());
    }

    [Button]
    public void ShowOres()
    {
        SetOresAllwaysVisible(true);
    }

    [Button]
    public void HideOres()
    {
        SetOresAllwaysVisible(false);
    }

    public void SetOresAllwaysVisible(bool visible)
    {
        oreTilemap.GetComponent<TilemapRenderer>().material =  visible? unlitMaterial : litMaterial;
        showOverlayAlways = visible;

        UpdateAllOres();
    }

    private void UpdateAllOres()
    {
        oreTilemap.ClearAllTiles();
        Util.IterateXY(SizeX, SizeY, (x,y) => 
        { 
        var oreTile = GetVisualOverlayTileFor(x, y);
        oreTilemap.SetTile(new Vector3Int(x, y, 0), oreTile);
        });
    }

    public override void RefreshAll()
    {
        base.RefreshAll();
        NotifyRecieversAll(TileUpdateReason.VisualUpdate);
    }
    private void NotifyRecieversAll(TileUpdateReason reason)
    {
        Util.IterateXY(SizeX, SizeY, (x, y) => NotifyRecieversOfUpdates(x, y, reason));
    }

    private IEnumerator UpdateUnstableTilesRoutine()
    {
        Debug.Log("Starting UnstableTiles routine");

        int i = 0;
        while (true)
        {
            if (unstableTiles.Count == 0)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }
            else if (i >= unstableTiles.Count)
            {
                yield return new WaitForSeconds(1f);
                i = 0;

                if (unstableTiles.Count == 0)
                    continue;
            }

            var current = unstableTiles[i];
            var t = this[current.Location];
            int x = current.Location.x;
            int y = current.Location.y;

            if (current.ShouldCollapse())
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
            else
            {
                i++;
            }
        }
    }

    public CrumbleType GetCrumbleTypeAt(int x, int y)
    {
        var t = this[x, y];
        if (t.Unstable)
            return CrumbleType.Unstable;

        return GetTileInfo(t.Type).CrumbleType;
    }

    protected override void BreakBlock(int x, int y, Tile t, DamageType damageType)
    {
        if (t.Unstable)
        {
            TryRemoveUnstableAt(new Vector2Int(x, y));
        }

        base.BreakBlock(x, y, t, damageType);

        if (damageType == DamageType.Mining)
        {
            TileInfo info = GetTileInfo(t.Type);
            if (info.ItemToDrop != ItemType.None)
            {
                inventoryManager.PlayerCollects(info.ItemToDrop, 1);
            }

            MinedBlock?.Invoke(t.Type);
        }
    }

    public override void SetMapAt(int x, int y, Tile value, TileUpdateReason reason, bool updateProperties = true, bool updateVisuals = true)
    {
        //On Place try mark neighbours unstable
        if (reason == TileUpdateReason.Place && value.Type != TileType.Air)
        {
            foreach (var tile in MapHelper.GetDirectNeighboursIndiciesOf(x, y))
            {
                Util.DebugDrawTile(tile);
                TryRemoveUnstableAt(tile);
            }
        }

        base.SetMapAt(x, y, value, reason, updateProperties, updateVisuals);
    }

    public GameObject InstantiateEntity(GameObject prefab, Vector3 position)
    {
        if (prefabFactory != null)
        {
            var transform = prefabFactory.Create(prefab, entitiesParent);
            transform.transform.localPosition = position;
            transform.GetComponent<ITileMapElement>()?.Setup(this);
            return transform.gameObject;
        }
        return null;
    }

    public void NotifyMirrorWorldSideChange(MirrorState newState)
    {
        MirrorSideChanged?.Invoke(newState);
    }



    public void MakeTileUnstable(Vector2Int location, float duration)
    {
        var tile = this[location];
        if (!tile.Unstable)
        {
            UnstableTile t = new UnstableTile();
            t.Location = location;
            t.Duration = duration;

            var go = prefabFactory.Create(MapSettings.CrumbleEffects, new Vector3(location.x + 0.5f, location.y), quaternion.identity, transform); //Safe
            go.GetComponent<CrumblingParticle>().SetDuration(duration);
            t.Effects = go.gameObject;

            unstableTiles.Add(t);
            tile.Unstable = true;
            this[location] = tile;
        }
    }

    public void TryRemoveUnstableAt(Vector2Int loc)
    {
        var t = this[loc];

        if (!t.Unstable)
            return;

        var i = FindUnstableMatching(loc);
        if (i >= 0)
        {
            RemoveUnstableTileAt(i);
        }
    }

    private int FindUnstableMatching(Vector2Int loc)
    {
        return unstableTiles.FindIndex((x) => x.Location == loc);
    }

    private void RemoveUnstableTileAt(int i)
    {
        var t = unstableTiles[i];
        Destroy(t.Effects);
        unstableTiles.RemoveAt(i);

        Tile tile = this[t.Location];
        tile.Unstable = false;
        this[t.Location] = tile;
    }

    public void SetReceiverMapAt(int x, int y, ITileUpdateReceiver receiver)
    {
        if (IsOutOfBounds(x, y))
            return;

        receiverMap[x, y] = receiver;
    }

    private IEnumerator RunCompleteGeneration()
    {
        DurationTracker tracker = new DurationTracker("Complete generation");
        Time.timeScale = 0;
        Populate();

        Util.IterateX(GenerationSettings.AutomataSteps, (x) => RunAutomataStep());

        PopulateOres();

        additiveCoveredMap = new bool[SizeX, SizeY];

        yield return sceneAdder.LoadAll();

        PupulateRocks();

        PopulateBorders();

        tracker.Stop();
        Time.timeScale = 1;
        gameState.ChangeStateTo(GameState.State.PostLoadScenes);
    }

    public bool IsAdditivelyCoveredAt(int x, int y)
    {
        if (additiveCoveredMap == null || IsOutOfBounds(x, y))
        {
            return false;
        }

        return additiveCoveredMap[x, y];
    }

    public bool IsAdditivelyCoveredAtAny(List<Vector2Int> locations)
    {
        foreach (var l in locations)
        {
            if (IsAdditivelyCoveredAt(l.x, l.y))
            {
                return true;
            }
        }
        return false;
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
        int x = UnityEngine.Random.Range(0, SizeX - 1);

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
            if (!IsAdditivelyCoveredAtAny(locations))
            {
                foreach (var loc in locations)
                {
                    SetMapAt(loc.x, loc.y, Tile.Air, TileUpdateReason.Generation, updateProperties: false, updateVisuals: false);
                }

                Vector3 pos = new Vector3(x + pass.Size.x * 0.5f, y + pass.Size.y * 0.5f);
                var go = InstantiateEntity(pass.Prefab, pos);
            }
        }
    }

    public void CollapseAt(int x, int y, bool updateVisuals)
    {
        WrapXIfNecessary(ref x);
        Tile t = this[x, y];
        SetMapAt(x, y, Tile.Air, TileUpdateReason.Collapse, updateProperties: true, updateVisuals);

        var go = prefabFactory.Create(GenerationSettings.PhysicalTilePrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
        go.GetComponent<PhysicalTile>().Setup(this, t, GetTileInfo(t.Type));
    }

    protected override void UpdatePropertiesAt(int x, int y, Tile newTile, Tile previousTile, TileUpdateReason reason)
    {
        base.UpdatePropertiesAt(x, y, newTile, previousTile, reason);

        if ((reason & TileUpdateReason.DoNotUpdateReceivers) != TileUpdateReason.None)
            return;

        NotifyRecieversOfUpdates(x, y, reason);

        foreach (var n in MapHelper.GetNeighboursIndiciesOf(x, y))
            NotifyRecieversOfUpdates(n.x, n.y, TileUpdateReason.VisualUpdate);

        foreach (var n in MapHelper.Get2ndDegreeNeighboursIndiciesOf(x, y))
            NotifyRecieversOfUpdates(n.x, n.y, TileUpdateReason.VisualUpdate);
    }

    public void NotifyRecieversOfUpdates(int x, int y, TileUpdateReason reason)
    {
        WrapXIfNecessary(ref x);
        if (IsOutOfBounds(x, y))
            return;

        if ((reason & TileUpdateReason.VisualUpdate) == TileUpdateReason.None)
        {
            receiverMap[x, y]?.OnTileChanged(x, y, reason);
            receiverMap[x, y] = null;
        }
        else
        {
            receiverMap[x, y]?.OnTileUpdated(x, y);
        }
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
                Util.IterateX((int)(SizeX * pass.Probability.Evaluate((float)y / SizeY) * 0.01f), (x) => TryPlaceVein(pass.TileType, Util.RandomInVector(pass.OreVeinSize), y, pass));
            }
        }
    }

    private void TryPlaceVein(TileType type, int amount, int y, OrePass orePass)
    {

        int x = UnityEngine.Random.Range(0, SizeX);

        GrowVeinAt(x, y, type, amount, orePass);
    }

    private void GrowVeinAt(int startX, int startY, TileType tile, int amount, OrePass orePass)
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
                    var dir = Util.RandomDirectionWeighted(orePass.DirectionProportions.x, orePass.DirectionProportions.y);
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
        Tile t = Tile.Make(TileType.Stone);
        this[x, y] = t;
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

        this[x, y] = IsBlockAt(x, y) ? (nbs > GenerationSettings.DeathLimit ? Tile.Make(TileType.Stone) : Tile.Air) : (nbs > GenerationSettings.BirthLimit ? Tile.Make(TileType.Stone) : Tile.Air);
    }

    public override string GetSaveID()
    {
        return saveID;
    }

    protected override void AdditiveLoadAt(BaseMapSaveData loadedData, int x, int y, int xOffset, int yOffset)
    {
        base.AdditiveLoadAt(loadedData, x, y, xOffset, yOffset);
        var t = loadedData.Map[x, y];

        if (t.Type != TileType.Ignore && !IsOutOfBounds(x + xOffset, y + yOffset))
        {
            additiveCoveredMap[x + xOffset, y + yOffset] = true;
        }
    }

    public enum MirrorState { Center, Right, Left };
}
