using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryOwner
{
    Inventory Inventory { get; }
}

public class InventoryOwner : MonoBehaviour, IInventoryOwner, IInteractable
{
    [Header("Inventory Owner")]
    [SerializeField] Inventory inventory;
    [SerializeField] Canvas canvas;
    [SerializeField] InventoryVisualizer inventoryVisualizerPrefab;
    [SerializeField] AudioSource openSource;

    InventoryVisualizer inventoryVisualizer;
    InventoryState state = InventoryState.Closed;
    public Inventory Inventory { get => inventory; }
    public InventoryState InventoryDisplayState { get => state; }

    protected virtual void Start()
    {
        inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        if (state == InventoryState.Open && inventoryVisualizer != null)
            inventoryVisualizer.RefreshInventoryDisplay();
    }

    public void OpenInventory()
    {
        if (state == InventoryState.Closed)
        {
            state = InventoryState.Open;
            if (inventoryVisualizer == null)
            {
                if (openSource != null)
                {
                    openSource.pitch = 1;
                    openSource.Play();
                }

                inventoryVisualizer = Instantiate(inventoryVisualizerPrefab, canvas.transform);
                inventoryVisualizer.Init(transform, inventory);
            }

            InventoryManager.NotifyInventoryOpen(this);
        }
    }

    public void CloseInventory()
    {
        if (state == InventoryState.Open)
        {
            state = InventoryState.Closed;
            if (openSource != null)
            {
                openSource.pitch = 0.66f;
                openSource.Play();
            }

            if (inventoryVisualizer != null)
            {
                inventoryVisualizer.Close();
                inventoryVisualizer = null;
            }
        }

        InventoryManager.NotifyInventoryClosed(this);
    }

    public void BeginInteracting(GameObject interactor)
    {
        OpenInventory();
    }

    public void EndInteracting(GameObject interactor)
    {
        CloseInventory();
    }
}

public enum InventoryState
{
    Closed,
    Open
}
