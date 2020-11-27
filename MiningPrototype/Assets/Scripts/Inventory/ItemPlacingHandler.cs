using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacingHandler : MonoBehaviour
{
    [Zenject.Inject] PlayerInteractionHandler player;
    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] Zenject.DiContainer diContainer;

    bool holdingPlacable;
    ItemAmountPair currentHeld;

    Transform previewTransform;
    IItemPreview preview;

    IDropReceiver currentReceiver;

    public void Hide()
    {
        player.SetHeldItem(setToPickaxe: true);

        if (previewTransform != null)
            Destroy(previewTransform.gameObject);
        preview = null;

        if (currentReceiver != null)
            currentReceiver.EndHover();
    }

    public void Show(ItemAmountPair pair)
    {
        currentHeld = pair;
        var info = ItemsData.GetItemInfo(pair.type);

        if (info.CanBePlaced)
        {
            if (info.PickupPreviewPrefab != null)
            {
                player.SetHeldItem(setToPickaxe: false);
                player.SetHeldItemSprite(info.PickupHoldSprite);

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

    public void TryPlace(ItemType type, Vector3 tryplacePosition)
    {
        if (currentReceiver != null)
        {
            if (currentReceiver.WouldTakeDrop(currentHeld))
            {
                currentReceiver.ReceiveDrop(currentHeld);
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
                    if (InventoryManager.PlayerTryPay(type, 1))
                    {
                        var go = diContainer.InstantiatePrefab(info.Prefab, preview.GetPlacePosition(tryplacePosition), Quaternion.identity, null);
                    }
                }
            }
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

    public bool IsAboveReceiver()
    {
        return currentReceiver != null && currentReceiver.WouldTakeDrop(currentHeld);
    }
}
