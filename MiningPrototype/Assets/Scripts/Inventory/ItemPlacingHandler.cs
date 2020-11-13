using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacingHandler : Singleton<ItemPlacingHandler>
{

    [SerializeField] PlayerController player;
    Transform previewTransform;
    IItemPreview preview;

    public void Hide()
    {
        player.SetHeldItem(setToPickaxe: true);

        if (previewTransform != null)
            Destroy(previewTransform.gameObject);
        preview = null;
    }

    public void Show(ItemType type)
    {
        var info = ItemsData.GetItemInfo(type);

        if (info.PickupPreviewPrefab != null)
        {
            player.SetHeldItem(setToPickaxe: false);
            player.SetHeldItemSprite(info.PickupHoldSprite);

            var go = Instantiate(info.PickupPreviewPrefab);
            previewTransform = go.transform;
            preview = previewTransform.GetComponent<IItemPreview>();
        }
    }

    public void TryPlace(ItemType type, Vector3 tryplacePosition)
    {
        if (preview != null)
        {
            var info = ItemsData.GetItemInfo(type);
            if (info.CanBePlaced && info.Prefab != null)
            {
                if (InventoryManager.PlayerTryPay(type, 1))
                {
                    var go = Instantiate(info.Prefab, preview.GetPlacePosition(tryplacePosition), Quaternion.identity);
                }
            }
        }

    }

    public void UpdatePosition(Vector3 position)
    {
        if (preview != null)
            preview.UpdatePreview(position);
    }

}
