using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacingHandler : Singleton<ItemPlacingHandler>
{

    [SerializeField] PlayerController player;
    Transform preview;


    public void Hide()
    {
        player.SetHeldItem(setToPickaxe: true);

        if (preview != null)
            Destroy(preview.gameObject);
    }

    public void Show(ItemType type)
    {
        var info = ItemsData.GetItemInfo(type);
        player.SetHeldItem(setToPickaxe: false);
        player.SetHeldItemSprite(info.PickupSprite);

        var go = new GameObject("TempPreview");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = info.PickupSprite;
        preview = go.transform;
    }

    public void UpdatePosition(Vector3 position)
    {
        if (preview != null)
            preview.position = position;
    }

}
