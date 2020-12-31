using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngineInternal;
using Zenject;

public class PlayerInteractionHandler : InventoryOwner, IDropReceiver
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
    IInteractable currentInteractable;
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
    }

    private void OnDeath()
    {
        TryDisableMiningVisuals();
    }

    private void Update()
    {
        //bool mouseInInventoryRange = Vector3.Distance(GetPositionInGridV3(), GetClickPositionV3()) <= settings.inventoryOpenDistance;

        if (player.CanUseInventory())
        {

            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E))
            {
                ToggleInventory();
            }

            //xif (Input.GetMouseButtonDown(1))
            //x{
            //x    PlayerActivity?.Invoke();
            //x    if (mouseInInventoryRange)
            //x    {
            //x        ToggleInventory();
            //x    }
            //x}
        }

        //Stop interacting when too far away
        if (CurrentInteractableIsValid() && Vector3.Distance(transform.position, currentInteractable.gameObject.transform.position) > settings.maxInteractableDistance)
        {
            TryStopInteracting();
        }

        if (player.CanInteract())
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

                    //if (!mouseInInventoryRange)
                    //{

                    if (!CurrentInteractableIsValid())
                    {
                        TryInteract();
                    }
                    else
                    {
                        if (eventSystem.IsPointerOverGameObject() == false)
                            TryStopInteractingIfHover();
                    }
                    //}
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

            if (!CurrentInteractableIsValid())
                PlayerActivity?.Invoke();
        }
        else
        {
            gridDigTarget = null;
            TryDisableMiningVisuals();
        }

        UpdateDigHighlight();
    }

    private bool CurrentInteractableIsValid()
    {
        return currentInteractable != null && !currentInteractable.Equals(null);
    }

    private void UpdateHover(bool hasDigTarget)
    {
        var hits = Util.RaycastFromMouse(cameraController.Camera);

        IHoverable newHover = null;

        if (!hasDigTarget)
        {
            foreach (var hit in hits)
            {
                if (hit.transform.TryGetComponent(out IHoverable hoverable))
                {
                    //Debug.Log(hit.transform.name);
                    newHover = hoverable;

                    

                    break;
                }
            }

            //hover over something interactable
            if ((newHover == null || !newHover.IsInteractable) && !eventSystem.IsPointerOverGameObject())
                cursorHandler.SetCursor(CursorType.Default);
            else
                cursorHandler.SetCursor(CursorType.Interactable);

        }
        else
        {
            if (!eventSystem.IsPointerOverGameObject() && gameState.CurrentState != GameState.State.Paused)
                cursorHandler.SetCursor(CursorType.Mining);
        }

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

        currentInteractable = null;
        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            //begin interaction
            if (hit.transform.TryGetComponent(out IBaseInteractable baseInteractable))
            {
                if(baseInteractable is IInteractable interactable1)
                {
                    if (hover != null)
                        hover.HoverExit();
                    StartInteractionWith(interactable1);
                    break;
                }
                else
                {
                    baseInteractable.BeginInteracting(gameObject);
                }
            }
        }
    }

    private void StartInteractionWith(IInteractable interactable)
    {
        Debug.Log("Started interacting with: " + interactable.gameObject.name);
        currentInteractable = interactable;
        currentInteractable.SubscribeToForceQuit(OnInteractableForceQuit);
        currentInteractable.BeginInteracting(gameObject);
    }

    public void ForceInteractionWith(IInteractable interactable)
    {
        TryStopInteracting();
        StartInteractionWith(interactable);
    }

    private void OnInteractableForceQuit()
    {
        TryStopInteracting();
    }

    private bool UpdateDigTarget()
    {
        previousGridDigTarget = gridDigTarget;
        gridDigTarget = MapHelper.GetMiningTarget(map, GetPositionInGridV3(), GetClickCoordinate());
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
                mouseHighlight.position = new Vector3(nonGridDigTarget.GetPosition().x, nonGridDigTarget.GetPosition().y, 0);
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
            return (!player.InOverworld() || info.MinableInOverworld);
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
                CloseInventory();
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

    private void TryStopInteracting()
    {
        if (CurrentInteractableIsValid())
        {
            currentInteractable.UnsubscribeToForceQuit(OnInteractableForceQuit);
            currentInteractable.EndInteracting(gameObject);
            currentInteractable = null;
        }
    }

    private void TryStopInteractingIfHover()
    {
        var hits = Util.RaycastFromMouse(cameraController.Camera, settings.interactionMask.value);

        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                if (interactable == currentInteractable)
                {
                    TryStopInteracting();
                }
            }
        }
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
        return new Vector3(transform.position.x, transform.position.y + 1.4f); //+1 to be at center of player
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
        return true;
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
}
