using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngineInternal;


public class PlayerInteractionHandler : InventoryOwner
{
    [SerializeField] PlayerSettings settings;
    [SerializeField] PlayerStateMachine player;

    [SerializeField] GameObject pickaxe;
    [SerializeField] Transform mouseHighlight;

    [SerializeField] AudioSource breakBlock, startMining;
    [SerializeField] PickaxeAnimator pickaxeAnimator;

    [SerializeField] EventSystem eventSystem;
    [SerializeField] ParticleSystem miningParticles;
    [SerializeField] SpriteRenderer heldItemPreview;

    SpriteAnimator spriteAnimator;

    Camera camera;
    SpriteRenderer spriteRenderer;
    Vector2Int? gridDigTarget;
    IInteractable currentInteractable;
    IMinableNonGrid nonGridDigTarget;

    [ReadOnly]
    [SerializeField] bool inMining;

    private bool heldIsPickaxe = true;

    public event System.Action PlayerActivity;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteAnimator = GetComponent<SpriteAnimator>();
    }

    private void Update()
    {
        bool mouseInInventoryRange = Vector3.Distance(GetPositionInGridV3(), GetClickPositionV3()) <= settings.inventoryOpenDistance;

        if (player.CanUseInventory())
        {

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }

            if (Input.GetMouseButtonDown(1))
            {
                PlayerActivity?.Invoke();
                if (mouseInInventoryRange)
                {
                    ToggleInventory();
                }
            }
        }

        //Stop interacting when too far away
        if (currentInteractable != null && Vector3.Distance(transform.position, currentInteractable.gameObject.transform.position) > settings.maxInteractableDistance)
        {
            TryStopInteracting();
        }

        if (player.CanInteract())
        {
            if (Vector2Int.Distance(GetPositionInGrid(), GetClickCoordinate()) <= settings.maxDigDistance)
            {
                UpdateDigTarget();
                UpdateNonGridDigTarget();

                if (Input.GetMouseButton(0))
                {
                    if (eventSystem.currentSelectedGameObject == null)
                        TryDig();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    PlayerActivity?.Invoke();

                    if (!mouseInInventoryRange)
                    {
                        if (currentInteractable == null)
                        {
                            TryInteract();
                        }
                        else
                        {
                            if (eventSystem.IsPointerOverGameObject() == false)
                                TryStopInteracting();
                        }
                    }
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
            UpdateDigHighlight();
        }
    }

    private void ToggleInventory()
    {
        if (InventoryDisplayState == InventoryState.Closed)
            OpenInventory();
        else
            CloseInventory();
    }

    private void TryInteract()
    {
        var hits = Util.RaycastFromMouse();

        currentInteractable = null;
        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                Debug.Log(hit.transform.name);
                currentInteractable = interactable;
                currentInteractable.SubscribeToForceQuit(OnInteractableForceQuit);
                currentInteractable.BeginInteracting(gameObject);
                Debug.DrawLine(GetPositionInGridV3(), hit.point, Color.green, 1f);
                break;
            }
        }
    }

    private void OnInteractableForceQuit()
    {
        TryStopInteracting();
    }

    private void UpdateDigTarget()
    {
        gridDigTarget = TileMapHelper.GetMiningTarget(Map.Instance, GetPositionInGrid(), GetClickCoordinate());
        if (!Map.Instance.CanTarget(gridDigTarget.Value.x, gridDigTarget.Value.y))
        {
            gridDigTarget = null;
        }
    }


    private void UpdateNonGridDigTarget()
    {
        var hits = Util.RaycastFromMouse();

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
    }

    private void UpdateDigHighlight()
    {

        if (gridDigTarget == null)
        {
            if (nonGridDigTarget == null)
                mouseHighlight.position = new Vector3(-1000, -1000);
            else
                mouseHighlight.position = new Vector3(nonGridDigTarget.GetPosition().x, nonGridDigTarget.GetPosition().y, 0);
        }
        else
            mouseHighlight.position = new Vector3(gridDigTarget.Value.x, gridDigTarget.Value.y, 0) + new Vector3(0.5f, 0.5f, 0);
    }

    private void TryPlace()
    {
        Vector2Int clickPos = GetClickCoordinate();
        if (TileMapHelper.HasLineOfSight(Map.Instance, GetPositionInGrid(), clickPos, debugVisualize: true))
            Map.Instance.PlaceAt(clickPos.x, clickPos.y, Tile.Make(TileType.Stone));
    }

    private void TryDig()
    {
        if (gridDigTarget.HasValue)
        {
            Tile t = Map.Instance[gridDigTarget.Value];
            var info = TilesData.GetTileInfo(t.Type);

            if (!player.InOverworld() || info.MinableInOverworld)
            {
                CloseInventory();
                PlayerActivity?.Invoke();

                bool broken = Map.Instance.DamageAt(gridDigTarget.Value.x, gridDigTarget.Value.y, Time.deltaTime * settings.digSpeed * ProgressionHandler.Instance.DigSpeedMultiplyer, playerCaused: true);

                if (broken)
                {
                    miningParticles.transform.position = (Vector3Int)gridDigTarget + new Vector3(0.5f, 0.5f);
                    miningParticles.Emit(settings.miningBreakParticlesCount);
                    breakBlock.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                    breakBlock.Play();
                    TryDisableMiningVisuals();
                }
                else
                {
                    UpdateMiningParticlesPositions();
                }

                TryEnableMiningVisuals();
                player.NotifyPickaxeUse();
                player.SetFaceDirection(gridDigTarget.Value.x - transform.position.x > 0);
            }
        }
        else
        {
            if (nonGridDigTarget != null)
            {
                nonGridDigTarget.Damage(Time.deltaTime * settings.digSpeed);
                miningParticles.transform.position = nonGridDigTarget.GetPosition();
                TryEnableMiningVisuals();
            }
            else
            {
                TryDisableMiningVisuals();
            }
        }
    }


    private void UpdateMiningParticlesPositions()
    {
        miningParticles.transform.position = TileMapHelper.GetWorldLocationOfFreeFaceFromSource(Map.Instance, gridDigTarget.Value, GetPositionInGrid());
        Debug.DrawLine((Vector3Int)GetPositionInGrid(), miningParticles.transform.position, Color.yellow, 0.1f);
    }

    private void TryDisableMiningVisuals()
    {
        if (inMining)
        {
            inMining = false;
            var emission = miningParticles.emission;
            emission.rateOverTimeMultiplier = 0;
            startMining.Stop();
            pickaxeAnimator.Stop();
        }
    }

    private void TryStopInteracting()
    {
        if (currentInteractable != null)
        {
            currentInteractable.EndInteracting(gameObject);
            currentInteractable.UnsubscribeToForceQuit(OnInteractableForceQuit);
            currentInteractable = null;
        }
    }

    private void TryEnableMiningVisuals()
    {
        if (!inMining)
        {
            var emission = miningParticles.emission;
            emission.rateOverTimeMultiplier = settings.miningParticlesRateOverTime;
            inMining = true;
            startMining.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            startMining.Play();
            pickaxeAnimator.Play();
        }
    }

    public Vector2Int GetPositionInGrid()
    {
        return transform.position.ToGridPosition() + new Vector2Int(0, 1); //+1 to be at center of player
    }

    /// <summary>
    /// Just +1 in Y compared to transform.position
    /// </summary>
    public Vector3 GetPositionInGridV3()
    {
        return new Vector3(transform.position.x, transform.position.y + 1); //+1 to be at center of player
    }

    private Vector2Int GetClickCoordinate()
    {
        Vector3 clickPos = GetClickPositionV3();
        return new Vector2Int((int)clickPos.x, (int)clickPos.y);
    }

    private Vector3 GetClickPositionV3()
    {
        return Util.MouseToWorld();
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

    public void SetHeldItemSprite(Sprite sprite)
    {
        heldItemPreview.sprite = sprite;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector3Int)GetPositionInGrid(), settings.maxDigDistance);
        Gizmos.DrawWireSphere(GetPositionInGridV3(), settings.inventoryOpenDistance);
    }
}
