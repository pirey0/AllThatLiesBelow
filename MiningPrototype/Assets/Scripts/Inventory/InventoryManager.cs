using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager
{
    private Inventory playerInventory;
    PlayerInteractionHandler player;

    public event System.Action<ItemAmountPair> PlayerCollected;

    public InventoryManager(PlayerInteractionHandler playerInteraction)
    {
        player = playerInteraction;
        playerInventory = player.Inventory;
    }

    public void PlayerCollects(ItemType itemType, int amount)
    {
        if (playerInventory != null)
            playerInventory.Add(itemType, amount);

        PlayerCollected?.Invoke(new ItemAmountPair(itemType, amount));
    }

    public bool PlayerTryPay(ItemType itemType, int amount)
    {
        Debug.Log("Player try pay: " + itemType + " " + amount);
        return playerInventory.TryRemove(new ItemAmountPair(itemType, amount));
    }

    public bool PlayerHas(ItemType type, int amount)
    {
        return playerInventory.Contains(new ItemAmountPair(type, amount));
    }

    public void ForcePlayerInventoryClose()
    {
        player.CloseInventory();
    }

}
