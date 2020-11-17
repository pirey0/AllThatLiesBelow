using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacingHandler : Singleton<ItemPlacingHandler>
{

    [SerializeField] PlayerInteractionHandler player;

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

                var go = Instantiate(info.PickupPreviewPrefab);
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
        if (holdingPlacable)
        {
            if (preview != null)
            {
                var info = ItemsData.GetItemInfo(type);
                if (info.CanBePlaced && info.Prefab != null && preview.WouldPlaceSuccessfully())
                {
                    if (InventoryManager.PlayerTryPay(type, 1))
                    {
                        var go = Instantiate(info.Prefab, preview.GetPlacePosition(tryplacePosition), Quaternion.identity);
                    }
                }
            }
        }
        else
        {
            if(currentReceiver != null)
            {
                if (currentReceiver.WouldTakeDrop(currentHeld))
                {
                    currentReceiver.ReceiveDrop(currentHeld);
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
        else
        {
            var hits = Util.RaycastFromMouse();
            IDropReceiver dropReceiver = null;
            foreach (var hit in hits)
            {
                if (hit.transform.TryGetComponent(out IDropReceiver receiver))
                {
                    dropReceiver = receiver;
                    break;
                }
                Debug.Log(hit.transform.name);
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
    }

}
