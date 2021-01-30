using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngineInternal;
using Zenject;


public interface IPlayerInteraction : IInventoryOwner, IDropReceiver
{
    void ForceInteractionWith(IInteractable altar);
    void SetHeldItem(bool setToPickaxe);
    void SetHeldItemSprite(ItemInfo info);
    bool InDialog();
    void ToggleInventory();
    void CloseInventory();
}

public class PlayerInteractionHandler : InventoryOwner, IDropReceiver , IPlayerInteraction
{
    [SerializeField] PlayerSettings settings;
    [SerializeField] PlayerStateMachine player;

    [SerializeField] GameObject pickaxe;
    [SerializeField] Transform mouseHighlight;

    [SerializeField] AudioSource breakBlock, breakFilling, startMining, cantMine;
    [SerializeField] PickaxeAnimator pickaxeAnimator;

    [SerializeField] ParticleSystem miningParticles, cantmineParticles;
    [SerializeField] SpriteRenderer heldItemPreview;

    [Inject] ProgressionHandler progressionHandler;
    [Inject] CameraController cameraController;
    [Inject] EventSystem eventSystem;
    [Inject] RuntimeProceduralMap map;
    [Inject] ItemPlacingHandler itemPlacingHandler;
    [Inject] CursorHandler cursorHandler;

    PlayerVisualController visualController;
    Vector2Int? gridDigTarget, previousGridDigTarget;
    List<IInteractable> currentInteractables = new List<IInteractable>();
    IMinableNonGrid nonGridDigTarget;
    IHoverable hover;

    [ReadOnly]
    [SerializeField] bool inMining;

    private bool heldIsPickaxe = true;

    public event System.Action PlayerActivity;

    protected override void Start()
    {
        base.Start();
        visualController = GetComponent<PlayerVisualController>();
        player.PlayerDeath += OnDeath;
        StateChanged += OnInventoryStateChanged;
    }

    private void OnInventoryStateChanged(InventoryState newState)
    {
        if (newState == InventoryState.Open)
            uIsHandler.NotifyOpening(this);
        else
            uIsHandler.NotifyClosing(this);
    }

    private void OnDeath()
    {
        TryDisableMiningVisuals();
    }

    private void Update()
    {
        RemoveDistantInteractables();

        if (player.CanUseInventory())
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }

        if (player.CanInteract())
        {
            UpdateInteract();
        }
        else
        {
            gridDigTarget = null;
            TryDisableMiningVisuals();
        }

        UpdateDigHighlight();
    }

    private void RemoveDistantInteractables()
    {
        for (int i = 0; i < currentInteractables.Count; i++)
        {
            var current = currentInteractables[i];
            if (!Util.IsNullOrDestroyed(current) && Vector3.Distance(transform.position, current.gameObject.transform.position) > settings.maxInteractableDistance)
            {
                if (TryStopInteractingWith(current))
                    i--;
            }
        }
    }

    private void UpdateInteract()
    {
        if (Vector2Int.Distance(GetPositionInGrid(), GetClickCoordinate()) <= settings.maxDigDistance)
        {
            //Update Hover and only show when no dig target was found
            bool hasTarget = UpdateDigTarget();
            bool hasNonGridTarget = UpdateNonGridDigTarget();
            UpdateHover((hasTarget || hasNonGridTarget));

            if (Input.GetMouseButton(0))
            {
                if (eventSystem.currentSelectedGameObject == null && player.CanDig && !itemPlacingHandler.IsDraggingItem)
                    TryDig();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                PlayerActivity?.Invoke();
                TryInteract();
            }
            else
            {
                TryDisableMiningVisuals();
            }
        }
        else
        {
            gridDigTarget = null;
            TryDisableMiningVisuals();
        }

        if (InInteraction())
            PlayerActivity?.Invoke();
    }

    private bool InInteraction()
    {
        return currentInteractables.Count > 0;
    }

    private void UpdateHover(bool hasDigTarget)
    {
        var hits = Util.RaycastFromMouse(cameraController.Camera, settings.interactionMask.value);

        IHoverable newHover = null;

        foreach (var hit in hits)
        {
            if (hit.transform.TryGetComponent(out IHoverable hoverable))
            {
                newHover = hoverable;
                break;
            }
        }

        //hover over something interactable
        if ((newHover == null || !newHover.IsInteractable) && !eventSystem.IsPointerOverGameObject())
            cursorHandler.SetCursor(CursorType.Default);
        else if (hasDigTarget && !eventSystem.IsPointerOverGameObject() && gameState.CurrentState != GameState.State.Paused)
            cursorHandler.SetCursor(CursorType.Mining);
        else
            cursorHandler.SetCursor(CursorType.Interactable);

        if (newHover != hover)
        {
            if (hover != null)
                hover.HoverExit();
            if (newHover != null)
                newHover.HoverEnter(itemPlacingHandler.IsDraggingItem);

            hover = newHover;
        }

        if (hover != null)
            hover.HoverUpdate();
    }

    public void ToggleInventory()
    {
        if (InventoryDisplayState == InventoryState.Closed)
            OpenInventory();
        else
            CloseInventory();
    }

    private void TryInteract()
    {
        var hits = Util.RaycastFromMouse(cameraController.Camera, settings.interactionMask.value);

        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.TryGetComponent(out IBaseInteractable baseInteractable))
            {
                if (baseInteractable is IInteractable interactable1)
                {
                    if (hover != null)
                        hover.HoverExit();

                    if (currentInteractables.Contains(interactable1))
                        TryStopInteractingWith(interactable1);
                    else
                        StartInteractionWith(interactable1);
                    break;
                }
                else
                {
                    baseInteractable.BeginInteracting(player);
                }
            }
        }
    }

    private void StartInteractionWith(IInteractable interactable)
    {
        Debug.Log("Started interacting with: " + interactable.gameObject.name);
        currentInteractables.Add(interactable);
        interactable.SubscribeToForceQuit(OnInteractableForceQuit);
        interactable.BeginInteracting(player);
    }

    public void ForceInteractionWith(IInteractable interactable)
    {
        StartInteractionWith(interactable);
    }

    private void OnInteractableForceQuit(IInteractable i)
    {
        TryStopInteractingWith(i);
    }

    private bool UpdateDigTarget()
    {
        previousGridDigTarget = gridDigTarget;
        gridDigTarget = MapHelper.GetMiningTarget(map, GetPositionInGridV3(), GetClickCoordinate());
        Util.DebugDrawTile(gridDigTarget.Value, Color.yellow, 0.05f);
        if (!map.CanTarget(gridDigTarget.Value.x, gridDigTarget.Value.y))
        {
            var secondary = GetSecondaryClickCoordinate();
            if (map.CanTarget(secondary.x, secondary.y))
            {
                gridDigTarget = secondary;
                return true;
            }

            gridDigTarget = null;
            return false;
        }

        return true;
    }


    private bool UpdateNonGridDigTarget()
    {
        var hits = Util.RaycastFromMouse(cameraController.Camera);

        IMinableNonGrid newTarget = null;
        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.TryGetComponent(out IMinableNonGrid minable))
            {
                //Debug.Log(hit.transform.name);
                newTarget = minable;
                break;
            }
        }

        //Debug.Log("new target: " + (newTarget != null).ToString() + " / old target:" + (nonGridDigTarget != null).ToString());
        if (newTarget != nonGridDigTarget)
        {
            if (nonGridDigTarget != null)
                nonGridDigTarget.MouseLeave();

            if (newTarget != null)
                newTarget.MouseEnter();

            nonGridDigTarget = newTarget;
        }

        return nonGridDigTarget != null;
    }

    private void UpdateDigHighlight()
    {

        if (gridDigTarget == null)
        {
            if (Util.IsNullOrDestroyed(nonGridDigTarget))
                mouseHighlight.position = new Vector3(-1000, -1000);
            else
            {
                var pos = nonGridDigTarget.GetPosition();
                mouseHighlight.position = pos;
            }
        }
        else
        {
            if (CanMineDigTarget())
                mouseHighlight.position = new Vector3(gridDigTarget.Value.x, gridDigTarget.Value.y, 0) + new Vector3(0.5f, 0.5f, 0);
            else
                mouseHighlight.position = new Vector3(-1000, -1000);
        }


    }

    private void TryPlace()
    {
        Vector2Int clickPos = GetClickCoordinate();
        if (MapHelper.HasLineOfSight(map, GetPositionInGrid(), clickPos, debugVisualize: true))
            map.SetMapAt(clickPos.x, clickPos.y, Tile.Make(TileType.Stone), TileUpdateReason.Place);
    }

    private bool CanMineDigTarget()
    {
        if (gridDigTarget.HasValue)
        {
            var info = map.GetTileInfoAt(gridDigTarget.Value);
            return ((gridDigTarget.Value.y < Constants.OVERWORLD_START_Y) || info.MinableInOverworld);
        }
        else
        {
            return false;
        }
    }

    private void TryDig()
    {
        if (gridDigTarget.HasValue)
        {

            if (CanMineDigTarget())
            {
                PlayerActivity?.Invoke();
                visualController.ForceUpdate();

                var info = map.GetTileInfoAt(gridDigTarget.Value);
                bool broken = map.DamageAt(gridDigTarget.Value.x, gridDigTarget.Value.y, Time.deltaTime * settings.digSpeed * progressionHandler.DigSpeedMultiplyer, BaseMap.DamageType.Mining);

                if (broken)
                {
                    miningParticles.transform.position = (Vector3Int)gridDigTarget + new Vector3(0.5f, 0.5f);
                    miningParticles.Emit(settings.miningBreakParticlesCount);

                    if (info.Type == TileType.FillingStone)
                    {
                        breakFilling.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                        breakFilling.Play();
                    }
                    else
                    {
                        breakBlock.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                        breakBlock.Play();
                    }

                    TryDisableMiningVisuals();
                }
                else
                {
                    UpdateMiningParticlesPositions();
                }

                if (gridDigTarget != previousGridDigTarget)
                {
                    //If block is switched and is not the same
                    if (gridDigTarget == null || previousGridDigTarget == null || map[gridDigTarget.Value].Type != map[previousGridDigTarget.Value].Type)
                        TryDisableMiningVisuals();
                }

                TryEnableMiningVisuals(info.damageMultiplyer);
                player.NotifyPickaxeUse();
                player.SetFaceDirection(gridDigTarget.Value.x - transform.position.x > 0);
            }
        }
        else
        {
            if (nonGridDigTarget != null)
            {
                visualController.ForceUpdate();
                nonGridDigTarget.Damage(Time.deltaTime * settings.digSpeed);
                miningParticles.transform.position = nonGridDigTarget.GetPosition();
                TryEnableMiningVisuals();
                player.NotifyPickaxeUse();
            }
            else
            {
                TryDisableMiningVisuals();
            }
        }
    }


    private void UpdateMiningParticlesPositions()
    {
        Vector3 pos = MapHelper.GetWorldLocationOfFreeFaceFromSource(map, gridDigTarget.Value, GetPositionInGrid());
        miningParticles.transform.position = pos;
        cantmineParticles.transform.position = pos;
        Debug.DrawLine((Vector3Int)GetPositionInGrid(), miningParticles.transform.position, Color.yellow, 0.1f);
    }

    protected override void OnInventoryChanged(bool add, ItemAmountPair pair, bool playSound)
    {
        if (add && ItemsData.GetItemInfo(pair.type).IsUpgrade)
        {
            Inventory.TryRemove(pair);
            progressionHandler.Upgrade(pair.type);
        }
        else
        {
            base.OnInventoryChanged(add, pair, playSound);
        }
    }

    private void TryDisableMiningVisuals()
    {
        if (inMining)
        {
            inMining = false;

            var emission = miningParticles.emission;
            emission.rateOverTimeMultiplier = 0;

            emission = cantmineParticles.emission;
            emission.rateOverTimeMultiplier = 0;

            startMining.Stop();
            cantMine.Stop();
            pickaxeAnimator.Stop();
        }
    }

    public bool InDialog()
    {
        foreach (var i in currentInteractables)
        {
            if (i is Altar)
            {
                return true;
            }
        }
        return false;
    }

    private bool TryStopInteractingWith(IInteractable interactable)
    {
        if (!Util.IsNullOrDestroyed(interactable))
        {
            interactable.UnsubscribeToForceQuit(OnInteractableForceQuit);
            interactable.EndInteracting(player);
            currentInteractables.Remove(interactable);
            return true;
        }
        return false;
    }

    private void TryEnableMiningVisuals(float targetDamageMultiplier = 1)
    {
        if (!inMining)
        {
            inMining = true;

            if (targetDamageMultiplier == 0)
            {
                var emission = cantmineParticles.emission;
                emission.rateOverTimeMultiplier = settings.miningParticlesRateOverTime;
                cantMine.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                cantMine.Play();
            }
            else
            {
                var emission = miningParticles.emission;
                emission.rateOverTimeMultiplier = settings.miningParticlesRateOverTime;
                startMining.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                startMining.Play();
            }
            pickaxeAnimator.Play();
        }
    }

    public Vector2Int GetPositionInGrid()
    {
        return transform.position.ToGridPosition() + new Vector2Int(0, 1); //+1 to be at center of player
    }

    /// <summary>
    /// Just +1.4f in Y compared to transform.position
    /// </summary>
    public Vector3 GetPositionInGridV3()
    {
        return new Vector3(transform.position.x, transform.position.y + 1.4f); //+1.4 to be at center of player
    }

    private Vector2Int GetClickCoordinate()
    {
        Vector3 clickPos = GetClickPositionV3();
        return clickPos.ToGridPosition();
    }

    private Vector2Int GetSecondaryClickCoordinate()
    {
        Vector3 clickPos = GetClickPositionV3();
        float y = clickPos.y % 1;
        return clickPos.ToGridPosition() + new Vector2Int(0, y > 0.5f ? 1 : -1);
    }



    private Vector3 GetClickPositionV3()
    {
        return Util.MouseToWorld(cameraController.Camera);
    }


    public void SetHeldVisible(bool isVisible = true)
    {
        if (heldIsPickaxe)
        {
            if (isVisible != pickaxe.activeSelf)
            {
                pickaxe.SetActive(isVisible);
            }
            heldItemPreview.enabled = false;
        }
        else
        {
            if (isVisible != heldItemPreview.enabled)
            {
                heldItemPreview.enabled = true;
            }

            if (pickaxe.activeSelf)
                pickaxe.SetActive(false);
        }
    }

    public void SetHeldItem(bool setToPickaxe)
    {
        heldIsPickaxe = setToPickaxe;
        if (setToPickaxe)
            heldItemPreview.sprite = null;
    }

    public void SetHeldItemSprite(ItemInfo item)
    {
        heldItemPreview.sprite = item.PickupHoldSprite;
        heldItemPreview.transform.localPosition = item.PickupHoldOffset;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector3Int)GetPositionInGrid(), settings.maxDigDistance);
        Gizmos.DrawWireSphere(GetPositionInGridV3(), settings.inventoryOpenDistance);
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return Inventory.CanDeposit;
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        //
    }

    public void EndHover()
    {
        //
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair, Inventory origin)
    {
        if (origin.Contains(pair) && origin.TryRemove(pair))
            Inventory.Add(pair);
    }

    public bool IsSameInventory(Inventory inventory)
    {
        return inventory == Inventory;
    }
}
