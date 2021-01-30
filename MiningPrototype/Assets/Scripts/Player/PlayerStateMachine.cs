using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;


public interface IPlayerController
{
    Transform transform { get; }
    Rigidbody2D Rigidbody { get; }

    float GetHorizontalInputRaw();
    float GetVerticalInputRaw();
    bool CanDig { get; set; }
    bool InCinematicMode { get; set; }
    bool CinematicSlowWalk { get; set; }
    float CinematicHorizontal { get; set; }

    void TakeDamage(DamageStrength strength);

    bool InVehicle();
    void EnterVehicle(IVehicle vehicle);
    void ExitVehicle(IVehicle vehicle);
    void Disable();
    void Enable();

    event Action LeftOverworld;
    event Action EnteredOverworld;
    bool InOverworld();
}


[DefaultExecutionOrder(-20)]
public class PlayerStateMachine : BasePlayerStateMachine , IPlayerController
{
    [SerializeField] PlayerStateInfo[] statesCanInteract;
    [SerializeField] PlayerInteractionHandler playerInteraction;
    [SerializeField] ParticleSystem midasParticles;
    [SerializeField] bool inCinematicMode;
    [SerializeField] float cinematicHorizontal;
    [SerializeField] float cinematicVertical;

    [Inject] RuntimeProceduralMap map;
    [Inject] TransitionEffectHandler transitionEffectHandler;
    [Inject] InventoryManager inventoryManager;

    Dictionary<string, PlayerStateInfo> canInteractInStateMap;
    RuntimeProceduralMap.MirrorState currentMirrorLoc;
    bool canDig = true;
    IVehicle currentVehicle;
    bool lastInOverworld;

    public event System.Action EnteredOverworld;
    public event System.Action LeftOverworld;

    public float CinematicHorizontal { get => cinematicHorizontal; set => cinematicHorizontal = value; }
    public float CinematicVertical { get => cinematicVertical; set => cinematicVertical = value; }

    public bool InCinematicMode { get => inCinematicMode; set => inCinematicMode = value; }

    public bool CinematicSlowWalk { get => slowWalkMode; set => slowWalkMode = value; }

    public bool CanDig { get => canDig; set => canDig = value; }

    protected override float GetHorizontalInput()
    {
        if (currentVehicle != null && currentVehicle.ConsumesHorizontalInput())
            return 0;

        return GetHorizontalInputRaw();
    }

    //Returns Horizontal Input without Vehicle consuming input
    public float GetHorizontalInputRaw()
    {
        return inCinematicMode ? cinematicHorizontal : Input.GetAxis("Horizontal");
    }

    protected override float GetVerticalInput()
    {
        if (currentVehicle != null && currentVehicle.ConsumesVerticalInput())
            return 0;

        return GetVerticalInputRaw();
    }

    //Returns Vertical Input without Vehicle consuming input
    public float GetVerticalInputRaw()
    {
        if (inCinematicMode)
            return cinematicVertical;

        if (Input.GetKey(KeyCode.Space))
            return 1;

        return Input.GetAxis("Vertical");
    }

    protected override void Start()
    {
        base.Start();
        canInteractInStateMap = new Dictionary<string, PlayerStateInfo>();

        foreach (var val in statesCanInteract)
        {
            canInteractInStateMap.Add(val.StateName, val);
        }

        playerInteraction.PlayerActivity += NotifyActivity;
        lastInOverworld = InOverworld();
    }

    protected override void OnRealStart()
    {
        ForceToState("Idle");
    }

    protected override void OnNewGame()
    {
        var start = LocationIndicator.Find(IndicatorType.PlayerStart);
        if (start != null)
        {
            transform.position = start.transform.position;
            lastInOverworld = InOverworld();
        }
    }

    protected override void DeathEnter()
    {
        transitionEffectHandler.FadeOut(FadeType.Death);
        inventoryManager.ForcePlayerInventoryClose();
        base.DeathEnter();
    }


    protected override void DeathExit()
    {

        base.DeathExit();
    }

    protected override float GetJumpMultiplyer()
    {
        //Old altar reward, defaults to 1
        return 1;
    }

    protected override float GetSpeedMultiplyer()
    {
        if (gameState.CurrentState != GameState.State.Playing)
            return 0;

        //Old altar reward, defaulted to 1
        return 1;
    }

    protected override void BaseMoveUpdate(float horizontal, Vector2 movement)
    {
        base.BaseMoveUpdate(horizontal, movement);

        UpdateWorldMirroring();

        if (gameState.CurrentState == GameState.State.Playing && progressionHandler.IsMidas)
        {
            MidasUpdate();
        }

        var inOverworld = InOverworld();
        if (inOverworld != lastInOverworld)
        {
            if (inOverworld)
            {
                EnteredOverworld?.Invoke();
            }
            else
            {
                LeftOverworld?.Invoke();
            }

            lastInOverworld = InOverworld();
        }
    }

    private void MidasUpdate()
    {
        var pos = transform.position.ToGridPosition() + new Vector2Int(0, -1);

        Util.DebugDrawTile(pos);
        var t = map[pos];

        if (t.Type == TileType.Stone)
        {
            map.SetMapAt(pos.x, pos.y, Tile.Make(TileType.Gold), TileUpdateReason.Place, updateProperties: true, updateVisuals: true);
            midasParticles.Emit(8);
        }
    }

    private void UpdateWorldMirroring()
    {
        if (rigidbody.position.x < 0)
        {
            rigidbody.position = new Vector2(rigidbody.position.x + map.SizeX, rigidbody.position.y);
        }
        else if (rigidbody.position.x > map.SizeX)
        {
            rigidbody.position = new Vector2(rigidbody.position.x - map.SizeX, rigidbody.position.y);
        }

        var oldMirrorLoc = currentMirrorLoc;
        currentMirrorLoc = GetMirrorLocation();
        if (currentMirrorLoc != oldMirrorLoc)
        {
            map.NotifyMirrorWorldSideChange(currentMirrorLoc);
        }
    }

    private RuntimeProceduralMap.MirrorState GetMirrorLocation()
    {
        if (rigidbody.position.x < map.SizeX / 3)
        {
            return RuntimeProceduralMap.MirrorState.Left;
        }
        else if (rigidbody.position.x > map.SizeX * 2 / 3)
        {
            return RuntimeProceduralMap.MirrorState.Right;
        }
        else
        {
            return RuntimeProceduralMap.MirrorState.Center;
        }
    }


    public bool CanInteract()
    {
        if (inCinematicMode)
            return false;

        if (canInteractInStateMap.ContainsKey(CurrentStateName))
            return canInteractInStateMap[CurrentStateName].CanInteract;
        else
            Debug.LogError("No canInteract set for " + CurrentStateName);


        return false;
    }

    public bool CanUseInventory()
    {
        if (inCinematicMode)
            return false;

        if (canInteractInStateMap.ContainsKey(CurrentStateName))
            return canInteractInStateMap[CurrentStateName].CanInventory;
        else
            Debug.LogError("No canInventory set for " + CurrentStateName);


        return false;
    }

    protected override bool ShouldCrouch()
    {
        var pos = transform.position.ToGridPosition() + new Vector2Int(0, 3);
        Util.DebugDrawTile(pos, Color.red, Time.deltaTime);

        return map.IsNeighbourAt(pos.x, pos.y);
    }

    protected override bool InInventory()
    {
        return playerInteraction.InventoryDisplayState == InventoryState.Open;
    }

    public void EnterVehicle(IVehicle vehicle)
    {
        if (!Util.IsNullOrDestroyed(currentVehicle))
        {
            currentVehicle.LeftBy(this);
        }

        currentVehicle = vehicle;

        if (!Util.IsNullOrDestroyed(currentVehicle))
        {
            vehicle.EnteredBy(this);
        }
    }

    public void ExitVehicle(IVehicle vehicle)
    {
        if (currentVehicle == vehicle)
            EnterVehicle(null);
    }

    public bool InVehicle()
    {
        return !Util.IsNullOrDestroyed(currentVehicle);
    }

    public void MakeKinematic()
    {
        rigidbody.isKinematic = true;
    }

    public void MakeNotKinematic()
    {
        rigidbody.isKinematic = false;
    }

}