using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryVisualizer : ScalingUIElementBase
{
    [SerializeField] int widthInSlots, heightInSlots;

    [SerializeField] RectTransform boxTransform;
    [SerializeField] Vector2 basePadding;
    [SerializeField] Vector2 additonalSpacePerSlotNeeded;

    [SerializeField] InventorySlotVisualizer inventorySlotPrefab;
    [SerializeField] Transform gridBox;
    [SerializeField] Transform gridLayoutParent;
    SpriteRenderer spriteRendererToGetOrientatioFrom;

    Inventory inventory;

    public void Init(Transform target, Inventory inventoryToVisualize)
    {
        transformToFollow = target;
        inventory = inventoryToVisualize;
        spriteRendererToGetOrientatioFrom = target.GetComponent<SpriteRenderer>();

        bool flipX = (spriteRendererToGetOrientatioFrom != null && spriteRendererToGetOrientatioFrom.flipX) ? true : false;
        UpdatePosition(flipX);

        RefreshInventoryDisplay();
        StartCoroutine(ScaleCoroutine(scaleUp: true));
    }

    protected override void Update()
    {
        bool flipX = (spriteRendererToGetOrientatioFrom != null && spriteRendererToGetOrientatioFrom.flipX) ? true : false;
        UpdatePosition(flipX);
    }

    [Button]
    public void RefreshInventoryDisplay ()
    {
        foreach (Transform child in gridLayoutParent)
        {
            GameObject.Destroy(child.gameObject);
        }

        KeyValuePair<ItemType, int>[] content = inventory.GetContent();
        SpawnItemElements(content);
        RecalculateUISize(content.Length);
        RecalculateUIOrientation(spriteRendererToGetOrientatioFrom);
    }

    private void RecalculateUISize (int sizeCurrent)
    {
        int width = widthInSlots;
        int height = heightInSlots;

        //inventory smaller than one row
        if (sizeCurrent <= widthInSlots)
        {
            width = Mathf.Max(1,sizeCurrent);
            height = 1;
        } else
        {
            float h = sizeCurrent / width;

            if (h > width)
            {
                height = width;
                width = (int)h;
            }
        }

        boxTransform.sizeDelta = new Vector2(basePadding.x + additonalSpacePerSlotNeeded.x * width, basePadding.y + additonalSpacePerSlotNeeded.y * height);
    }

    public void RecalculateUIOrientation(SpriteRenderer spriteRendererToGetOrientatioFrom)
    {
        if (spriteRendererToGetOrientatioFrom == null)
            return;

        Vector3 flipX = new Vector3(spriteRendererToGetOrientatioFrom.flipX? -1 : 1, 1, 1);

        if (gridBox != null)
            gridBox.localScale = flipX;

        if(gridLayoutParent != null)
            gridLayoutParent.localScale = flipX;
    }

    private void SpawnItemElements(KeyValuePair<ItemType, int>[] itemsToVisualize)
    {
        for (int i = 0; i < itemsToVisualize.Length; i++)
        {
            InventorySlotVisualizer newSlot = Instantiate(inventorySlotPrefab, gridLayoutParent);
            newSlot.Display(itemsToVisualize[i]);

            //Old System
            //int index = i;
            //newSlot.SetButtonToSlot(() => InventoryManager.TryMove(inventory, index));
        }
    }

    internal void Close()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine(scaleUp:false));
    }
}
