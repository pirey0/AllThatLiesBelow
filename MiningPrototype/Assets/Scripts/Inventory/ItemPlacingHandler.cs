using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemPlacingHandler : MonoBehaviour
{
    [Zenject.Inject] PlayerInteractionHandler player;
    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] Zenject.DiContainer diContainer;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] EventSystem eventSystem;

    [SerializeField] AudioSource placingSound;

    bool holdingPlacable;
    ItemAmountPair currentHeld;
    Inventory currentOrigin;

    public bool IsDraggingItem
    {
        get => (currentHeld != null);
    }

    Transform previewTransform;
    IItemPreview preview;

    IDropReceiver currentReceiver;

    public event System.Action<ItemType> Placed;

    public void Hide()
    {
        player.SetHeldItem(setToPickaxe: true);

        if (previewTransform != null)
            Destroy(previewTransform.gameObject);
        preview = null;

        if (currentReceiver != null)
            currentReceiver.EndHover();
    }

    public void Show(ItemAmountPair pair, Inventory origin)
    {
        currentHeld = pair;
        currentOrigin = origin;
        var info = ItemsData.GetItemInfo(pair.type);

        if (info.CanBePlaced)
        {
            if (info.PickupPreviewPrefab != null)
            {
                player.SetHeldItem(setToPickaxe: false);
                player.SetHeldItemSprite(info);

                var go = diContainer.InstantiatePrefab(info.PickupPreviewPrefab);
                previewTransform = go.transform;
                preview = previewTransform.GetComponent<IItemPreview>();
            }
            holdingPlacable = true;
        }
        else
        {
            holdingPlacable = false;
        }
    }

    public void TryPlace(ItemType type, Vector3 tryplacePosition, int amount = -1)
    {
        if (currentReceiver != null && currentOrigin != null)
        {
            ItemAmountPair pair = amount > 0 ? new ItemAmountPair(currentHeld.type, amount) : currentHeld;
            if (currentReceiver.WouldTakeDrop(pair))
            {
                currentReceiver.ReceiveDrop(pair, currentOrigin);
                return;
            }
        }

        if (holdingPlacable)
        {
            if (preview != null)
            {
                var info = ItemsData.GetItemInfo(type);
                if (info.CanBePlaced && info.Prefab != null && preview.WouldPlaceSuccessfully())
                {
                    if (currentOrigin.TryRemove(new ItemAmountPair(type, 1)))
                    {
                        PlayEffectsFor(type, tryplacePosition);
                        var go = diContainer.InstantiatePrefab(info.Prefab, preview.GetPlacePosition(tryplacePosition), Quaternion.identity, null);
                        Placed?.Invoke(type);
                    }
                }
            }
        }
    }


    public void PlayEffectsFor(ItemType type, Vector3 position)
    {
        switch (type)
        {
            case ItemType.Torch:
                break;

            default:
                placingSound?.Play();
                cameraController.Shake(preview.GetPlacePosition(position), CameraShakeType.hill, 0.1f, 10f);
                break;
        }
    }


    public void UpdatePosition(Vector3 position)
    {
        if (holdingPlacable)
        {
            if (preview != null)
                preview.UpdatePreview(position);
        }

        var hits = Util.RaycastFromMouse(cameraController.Camera);
        IDropReceiver dropReceiver = null;
        foreach (var hit in hits)
        {
            if (hit.transform.TryGetComponent(out IDropReceiver receiver))
            {
                dropReceiver = receiver;
                break;
            }
        }

        if (dropReceiver == null)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            foreach (RaycastResult item in results)
            {
                if (dropReceiver == null)
                    dropReceiver = item.gameObject.GetComponentInParent<IDropReceiver>();
            }
        }


        if (dropReceiver != currentReceiver)
        {
            if (currentReceiver != null)
                currentReceiver.EndHover();

            if (dropReceiver != null)
                dropReceiver.BeginHoverWith(currentHeld);
            currentReceiver = dropReceiver;
        }
        else
        {
            if (currentReceiver != null)
                currentReceiver.HoverUpdate(currentHeld);
        }

    }

    public bool WouldBeReceived()
    {
        return currentReceiver != null && currentReceiver.WouldTakeDrop(currentHeld);
    }

    public bool IsAboveReceiver()
    {
        return currentReceiver != null;
    }
}
