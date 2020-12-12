using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryVisualizer : ScalingUIElementBase, IDropReceiver
{
    [SerializeField] int widthInSlots, heightInSlots, playerInvMaxHeight = 5;

    [SerializeField] RectTransform boxTransform;
    [SerializeField] Vector2 basePadding;
    [SerializeField] Vector2 additonalSpacePerSlotNeeded;

    [SerializeField] InventorySlotVisualizer inventorySlotPrefab;
    [SerializeField] Transform gridBox;
    [SerializeField] Image gridBoxImage;
    [SerializeField] Transform gridLayoutParent;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] bool updateContinously = true;
    [SerializeField] ImageSpriteAnimator animator;
    [SerializeField] SpriteAnimation closeAnimation;

    [Zenject.Inject] ReadableItemHandler readableItemHandler;
    [Zenject.Inject] InventorySlotVisualizer.Factory slotFactory;

    SpriteRenderer spriteRendererToGetOrientatioFrom;
    Inventory inventory;
    [SerializeField] List<InventorySlotVisualizer> slots = new List<InventorySlotVisualizer>();

    bool useCustomPlayerInventory;

    public void Init(Transform target, Inventory inventoryToVisualize, bool isPlayerInventory = false)
    {
        useCustomPlayerInventory = isPlayerInventory;
        //updateContinously = !isPlayerInventory;
        transformToFollow = isPlayerInventory ? FindObjectOfType<PlayerInventoryOpener>().transform : target;

        inventory = inventoryToVisualize;
        spriteRendererToGetOrientatioFrom = target.GetComponent<SpriteRenderer>();

        if (isPlayerInventory)
        {
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
            gridBoxImage.pixelsPerUnitMultiplier = 1;
        }

        CreateInventoryDisplay();
    }

    protected override void Update()
    {
        if (updateContinously)
        {
            bool flipX = (spriteRendererToGetOrientatioFrom != null && spriteRendererToGetOrientatioFrom.flipX) ? false : true;
            UpdatePosition(flipX);
            RecalculateUIOrientation(spriteRendererToGetOrientatioFrom);
        }
    }

    public void UpdateInventoryDisplay(bool add, ItemAmountPair pair)
    {
        if (add)
        {
            if (!ItemsData.GetItemInfo(pair.type).AmountIsUniqueID)
            {

                foreach (InventorySlotVisualizer slot in slots)
                {
                    ItemAmountPair existingPair = slot.GetPair();
                    if (existingPair.type == pair.type)
                    {
                        slot.Display(new ItemAmountPair(existingPair.type, existingPair.amount + pair.amount), inventory);
                        return;
                    }
                }
            }

            ItemAmountPair[] itemAmountPairs = new ItemAmountPair[1];
            itemAmountPairs[0] = pair;

            StartCoroutine(SpawnItemElements(itemAmountPairs, skipOpenDelay: true));

            RecalculateUISize(inventory.GetContent().Length);
            RecalculateUIOrientation(spriteRendererToGetOrientatioFrom);

            return;
        }
        else
        {
            if (ItemsData.GetItemInfo(pair.type).AmountIsUniqueID)
            {
                int i = slots.FindIndex(0, (x) => x.GetPair() == pair);
                if (i >= 0 && i < slots.Count)
                {
                    InventorySlotVisualizer slot = slots[i];
                    slots.RemoveAt(i);
                    Destroy(slot.gameObject);

                    RecalculateUISize(slots.Count);
                    RecalculateUIOrientation(spriteRendererToGetOrientatioFrom);
                }
            }
            else
            {
                foreach (InventorySlotVisualizer slot in slots)
                {
                    ItemAmountPair slotPair = slot.GetPair();
                    if (slotPair.type == pair.type && slotPair.amount >= pair.amount)
                    {
                        if (slotPair.amount == pair.amount)
                        {
                            RecalculateUISize(slots.Count-1);
                            RecalculateUIOrientation(spriteRendererToGetOrientatioFrom);
                            Destroy(slot.gameObject);
                            slots.Remove(slot);
                        } else
                        {
                            slot.Display(new ItemAmountPair(pair.type, slotPair.amount - pair.amount), inventory);
                        }

                        return;
                    }
                }
            }
        }
    }

    [Button]
    public void CreateInventoryDisplay ()
    {

        foreach (Transform child in gridLayoutParent)
        {
            GameObject.Destroy(child.gameObject);
        }

        ItemAmountPair[] content = inventory.GetContent();
        StopAllCoroutines();
        StartCoroutine(SpawnItemElements(content));
        RecalculateUISize(content.Length);
        RecalculateUIOrientation(spriteRendererToGetOrientatioFrom);
    }

    private void RecalculateUISize (int sizeCurrent)
    {
        int width = widthInSlots;
        int height = heightInSlots;

        if (useCustomPlayerInventory)
        {
            height = Mathf.Min(sizeCurrent, playerInvMaxHeight);
            width =  Mathf.CeilToInt((float)sizeCurrent / (float)playerInvMaxHeight);

            followOffset = new Vector2(width * additonalSpacePerSlotNeeded.x * -0.5f, 1f);
        }
        else
        {
            //inventory smaller than one row
            if (sizeCurrent <= widthInSlots)
            {
                width = Mathf.Max(1, sizeCurrent);
                height = 1;
            }
            else if (sizeCurrent <= 6)
            {
                width = 3;
                height = 2;
            }
            else
            {
                float h = (float)sizeCurrent / width;

                if (h > heightInSlots)
                {
                    height = Mathf.CeilToInt(h);
                }
            }
        }

        boxTransform.sizeDelta = new Vector2(basePadding.x + additonalSpacePerSlotNeeded.x * width, basePadding.y + additonalSpacePerSlotNeeded.y * height);
    }

    public void RecalculateUIOrientation(SpriteRenderer spriteRendererToGetOrientatioFrom)
    {
        if (spriteRendererToGetOrientatioFrom == null)
            return;

        Vector3 flipX = new Vector3(spriteRendererToGetOrientatioFrom.flipX? 1 : -1, 1, 1);

        if (gridBox != null)
            gridBox.localScale = flipX;

        if(gridLayoutParent != null)
            gridLayoutParent.localScale = flipX;
    }

    private IEnumerator SpawnItemElements(ItemAmountPair[] itemsToVisualize, bool skipOpenDelay = false)
    {
        if (!skipOpenDelay)
            yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < itemsToVisualize.Length; i++)
        {
            InventorySlotVisualizer newSlot = slotFactory.Create(inventorySlotPrefab);
            newSlot.transform.SetParent(gridLayoutParent, worldPositionStays: false);
            newSlot.Display(itemsToVisualize[i], inventory);
            slots.Add(newSlot);
            yield return new WaitForSeconds(0.1f);
        }
    }

    internal void Close()
    {
        animator.Play(closeAnimation);
        float closeLength = closeAnimation.GetLength();
        readableItemHandler.Hide();
        StopAllCoroutines();
        StartCoroutine(DestroyItemElements(closeLength));
        Invoke("Selfdestroy", closeLength);
    }

    private IEnumerator DestroyItemElements(float length)
    {
        float delay = (length * (2/3)) / (float)slots.Count; 

        foreach (InventorySlotVisualizer slot in slots)
        {
            yield return new WaitForSeconds(delay);
            slot.CloseInventory();
        }
    }
    private void Selfdestroy()
    {
        Destroy(gameObject);
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return inventory.allowStoreing;
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
            inventory.Add(pair);
    }

    public class Factory : Zenject.PlaceholderFactory<GameObject, InventoryVisualizer>
    {
    }
}
