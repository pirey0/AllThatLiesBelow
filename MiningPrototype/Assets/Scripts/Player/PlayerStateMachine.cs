using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;



[DefaultExecutionOrder(-20)]
public class PlayerStateMachine : BasePlayerStateMachine
{
    [SerializeField] PlayerStateInfo[] statesCanInteract;
    [SerializeField] PlayerInteractionHandler playerInteraction;
    [SerializeField] ParticleSystem midasParticles;
    [SerializeField] bool inCinematicMode;
    [SerializeField] float cinematicHorizontal;
    [SerializeField] float cinematicVertical;


    [Inject] ProgressionHandler progressionHandler;
    [Inject] RuntimeProceduralMap map;
    [Inject] TransitionEffectHandler transitionEffectHandler;
    [Inject] InventoryManager inventoryManager;

    Dictionary<string, PlayerStateInfo> canInteractInStateMap;
    RuntimeProceduralMap.MirrorState currentMirrorLoc;

    protected override float GetHorizontalInput()
    {
        return inCinematicMode ? cinematicHorizontal : Input.GetAxis("Horizontal");
    }

    protected override float GetVerticalInput()
    {
        return inCinematicMode ? cinematicVertical : Input.GetAxis("Vertical");
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
    }

    protected override void OnNewGame()
    {
        var start = LocationIndicator.Find(IndicatorType.PlayerStart);
        if (start != null)
        {
            transform.position = start.transform.position;
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
        transitionEffectHandler.FadeIn(FadeType.Nightmare);
        base.DeathExit();
    }

    protected override float GetJumpMultiplyer()
    {
        return progressionHandler.JumpMultiplyer;
    }

    protected override float GetSpeedMultiplyer()
    {
        if (gameState.CurrentState != GameState.State.Playing)
            return 0;

        return progressionHandler.SpeedMultiplyer;
    }

    protected override void BaseMoveUpdate(float horizontal, Vector2 movement)
    {
        base.BaseMoveUpdate(horizontal, movement);

        UpdateWorldMirroring();

        if (gameState.CurrentState == GameState.State.Playing && progressionHandler.IsMidas)
        {
            MidasUpdate();
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
        if (canInteractInStateMap.ContainsKey(CurrentStateName))
            return canInteractInStateMap[CurrentStateName].CanInteract;
        else
            Debug.LogError("No canInteract set for " + CurrentStateName);


        return false;
    }

    public bool CanUseInventory()
    {
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

}