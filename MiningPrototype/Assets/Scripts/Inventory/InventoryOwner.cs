using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryOwner
{
    Inventory Inventory { get; }
}

public class InventoryOwner : StateListenerBehaviour, IInventoryOwner, IInteractable
{
    [Header("Inventory Owner")]
    [SerializeField] Inventory inventory;
    [SerializeField] InventoryVisualizer inventoryVisualizerPrefab;
    [SerializeField] AudioSource openSource, receiveSource;
    [SerializeField] bool inventoryVisualizerUpdates;
    [SerializeField] bool isPlayerInventory;

    [Zenject.Inject] InWorldCanvas inWorld;
    [Zenject.Inject] InventoryVisualizer.Factory visualizerFactory;
    [Zenject.Inject] PlayerInventoryOpener playerInventoryOpener;

    private event System.Action ForceInterrupt;
    public event System.Action<InventoryState> StateChanged;

    InventoryVisualizer inventoryVisualizer;
    InventoryState state = InventoryState.Closed;
    public Inventory Inventory { get => inventory; }
    public InventoryState InventoryDisplayState { get => state; }

    public virtual bool IsFlipped { get => false; }

    public void SetInventory(Inventory newInventory)
    {
        inventory.InventoryChanged -= OnInventoryChanged;
        inventory = newInventory;
        inventory.InventoryChanged += OnInventoryChanged;
    }

    public Inventory GetInventory()
    {
        return inventory;
    }

    protected virtual void Start()
    {
        inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInventoryChanged(bool add, ItemAmountPair pair)
    {
        Debug.Log(name + ": Inventory Changed");
        if (state == InventoryState.Open && inventoryVisualizer != null)
            inventoryVisualizer.UpdateInventoryDisplay(add, pair);

        if (add && receiveSource != null)
            receiveSource.Play();
    }

    public virtual void OpenInventory()
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

                inventoryVisualizer = visualizerFactory.Create(inventoryVisualizerPrefab.gameObject);
                if (isPlayerInventory)
                    inventoryVisualizer.transform.SetParent(playerInventoryOpener.Canvas.transform, worldPositionStays: false);
                else
                    inventoryVisualizer.transform.SetParent(inWorld.Canvas.transform, worldPositionStays: false);
                inventoryVisualizer.Init(transform, inventory, isPlayerInventory);
                inventoryVisualizer.SetFollowOnUpdate(inventoryVisualizerUpdates);
            }
            StateChanged?.Invoke(state);
        }
    }

    private void OnDestroy()
    {
        if(inventoryVisualizer != null)
        {
            Destroy(inventoryVisualizer.gameObject);
        }
    }

    public virtual void CloseInventory()
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

            StateChanged?.Invoke(state);
            ForceInterrupt?.Invoke();
        }
    }

    public virtual void BeginInteracting(GameObject interactor)
    {
        OpenInventory();
    }

    public virtual void EndInteracting(GameObject interactor)
    {
        CloseInventory();
    }

    public void SubscribeToForceQuit(Action action)
    {
        ForceInterrupt += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        ForceInterrupt -= action;
    }
}

public enum InventoryState
{
    Closed,
    Open
}
