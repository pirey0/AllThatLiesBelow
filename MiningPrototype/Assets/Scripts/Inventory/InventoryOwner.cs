using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryOwner
{
    Inventory Inventory { get; }
}

public class InventoryOwner : StateListenerBehaviour, IInventoryOwner, IInteractable, ILayeredUI
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
    [Zenject.Inject] protected UIsHandler uIsHandler;

    private event Action<IInteractable> ForceInterrupt;
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

    public virtual Inventory GetInventory()
    {
        return inventory;
    }

    protected virtual void Start()
    {
        inventory.InventoryChanged -= OnInventoryChanged;
        inventory.InventoryChanged += OnInventoryChanged;
    }

    protected virtual void OnInventoryChanged(bool add, ItemAmountPair pair, bool playSound)
    {

        if (state == InventoryState.Open && inventoryVisualizer != null)
            inventoryVisualizer.UpdateInventoryDisplay(add, pair);

        if (playSound && add && receiveSource != null && gameState.CurrentState == GameState.State.Playing)
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

    public Vector3 GetPosition()
    {
        return transform.position;
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
            ForceInterrupt?.Invoke(this);
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

    public void SubscribeToForceQuit(Action<IInteractable> action)
    {
        ForceInterrupt += action;
    }

    public void UnsubscribeToForceQuit(Action<IInteractable> action)
    {
        ForceInterrupt -= action;
    }

    public void ForceClose()
    {
        CloseInventory();
    }
}

public enum InventoryState
{
    Closed,
    Open
}
