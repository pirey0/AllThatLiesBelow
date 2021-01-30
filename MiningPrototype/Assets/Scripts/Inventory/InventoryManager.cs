using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : StateListenerBehaviour
{
    [Zenject.Inject] PlayerManager playerManager;

    public event System.Action<ItemAmountPair> PlayerCollected;

    protected override void OnRealStart()
    {
        playerManager.GetPlayerInventory().InventoryChanged += OnInventoryChanged;
    }

    private void OnInventoryChanged(bool add, ItemAmountPair element, bool playsound)
    {
        if (add)
        {
            PlayerCollected?.Invoke(element);
        }
    }

    public void PlayerCollects(ItemType itemType, int amount)
    {
        playerManager.GetPlayerInventory().Add(itemType, amount, playSound: false);
    }

    public void PlayerCollects(ItemAmountPair[] itemAmountPair)
    {
        foreach (ItemAmountPair pair in itemAmountPair)
        {
            PlayerCollects(pair.type, pair.amount);
        }
    }
    public bool PlayerTryPay(ItemType itemType, int amount)
    {
        Debug.Log("Player try pay: " + itemType + " " + amount);
        return playerManager.GetPlayerInventory().TryRemove(new ItemAmountPair(itemType, amount));
    }

    public bool PlayerHas(ItemType type, int amount)
    {
        return playerManager.GetPlayerInventory().Contains(new ItemAmountPair(type, amount));
    }

    public void ForcePlayerInventoryClose()
    {
        playerManager.GetPlayerInteraction().CloseInventory();
    }
}
