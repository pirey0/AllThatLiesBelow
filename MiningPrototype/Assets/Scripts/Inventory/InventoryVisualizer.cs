using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryVisualizer : MonoBehaviour
{
    [SerializeField] int widthInSlots, heightInSlots;

    [SerializeField] RectTransform boxTransform;
    [SerializeField] Vector2 basePadding;
    [SerializeField] Vector2 additonalSpacePerSlotNeeded;

    [SerializeField] InventorySlotVisualizer inventorySlotPrefab;
    [SerializeField] Transform gridLayoutParent;

    [SerializeField] Transform transformToFollow;
    [SerializeField] Vector2 followOffset;

    [SerializeField] Vector3 targetScale;
    [SerializeField] float openTimeMultiplier;

    Inventory inventory;

    public void Init(Transform target, Inventory inventoryToVisualize)
    {
        transformToFollow = target;
        inventory = inventoryToVisualize;
        RefreshInventoryDisplay();
        StartCoroutine(ScaleCoroutine(scaleUp: true));
    }

    private void Update()
    {
        if (transformToFollow != null)
        {
            transform.position = new Vector3(transformToFollow.position.x + followOffset.x, transformToFollow.position.y + followOffset.y, 0);
        }
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

    private void SpawnItemElements(KeyValuePair<ItemType, int>[] itemsToVisualize)
    {
        for (int i = 0; i < itemsToVisualize.Length; i++)
        {
            InventorySlotVisualizer newSlot = Instantiate(inventorySlotPrefab, gridLayoutParent);
            newSlot.Display(itemsToVisualize[i]);

            //Set button so click tries to move items
            int index = i;
            newSlot.SetButtonToSlot(() => InventoryManager.TryMove(inventory, index));
        }
    }

    internal void Close()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine(scaleUp:false));
    }

    IEnumerator ScaleCoroutine (bool scaleUp = true)
    {
        while (scaleUp && transform.localScale.magnitude <= targetScale.magnitude || !scaleUp && transform.localScale.x > 0f)
        {
            transform.localScale = transform.localScale + Vector3.one * Time.deltaTime * openTimeMultiplier * (scaleUp ? 1 : -1);
            yield return null;
        }

        if (scaleUp)
            transform.localScale = targetScale;
        else
            Destroy(gameObject);
    }
}
