using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryOwner
{
    Inventory Inventory { get; }
}

public class InventoryOwner : MonoBehaviour, IInventoryOwner , IInteractable
{
    [Header("Inventory Owner")]
    [SerializeField] Inventory inventory;
    [SerializeField] Canvas canvas;
    [SerializeField] InventoryVisualizer inventoryVisualizerPrefab;
    [SerializeField] InventoryVisualizer inventoryVisualizer;
    [SerializeField] AudioSource openSource;

    InventoryState state = InventoryState.Closed;
    public Inventory Inventory { get => inventory; }
    public InventoryState InventoryDisplayState { get => state; }

    public void OpenInventory()
    {
        if (state == InventoryState.Closed)
        {
            state = InventoryState.Open;
            if (inventoryVisualizer == null)
            {
                openSource.pitch = 1;
                openSource.Play();

                inventoryVisualizer = Instantiate(inventoryVisualizerPrefab, canvas.transform);
                inventoryVisualizer.Init(transform, inventory);
            }
        }
    }

    public void CloseInventory()
    {
        if (state == InventoryState.Open)
        {
            state = InventoryState.Closed;
            openSource.pitch = 0.66f;
            openSource.Play();

            if (inventoryVisualizer != null)
            {
                inventoryVisualizer.Close();
                inventoryVisualizer = null;
            }
        }
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
