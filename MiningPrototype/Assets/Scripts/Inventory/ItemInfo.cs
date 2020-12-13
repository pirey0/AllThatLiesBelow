using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "InventoryElement")]
public class ItemInfo : ScriptableObject
{
    public ItemType ItemType;

    public string DisplayName;
    public string DisplayTooltip;
    public Sprite DisplaySprite;
    public Sprite DisplaySpriteRead;

    public int Weight;

    [Header("PlacableObject")]
    public bool CanBePlaced;
    public Sprite PickupHoldSprite;
    public Vector2 PickupHoldOffset;
    public GameObject PickupPreviewPrefab;
    public GameObject Prefab;

    [Header("ReadableItem")]
    public bool AmountIsUniqueID;

    [Header("Burnable")]
    public bool IsBurnable;

    [Header("Altar Words used")]
    public string[] AltarWords;
}
