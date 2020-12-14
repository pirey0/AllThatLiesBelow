using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    [Zenject.Inject] PlayerInteractionHandler player;

    public event System.Action<ItemAmountPair> PlayerCollected;

    public void PlayerCollects(ItemType itemType, int amount)
    {
        player.Inventory.Add(itemType, amount);

        PlayerCollected?.Invoke(new ItemAmountPair(itemType, amount));
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
        return player.Inventory.TryRemove(new ItemAmountPair(itemType, amount));
    }

    public bool PlayerHas(ItemType type, int amount)
    {
        return player.Inventory.Contains(new ItemAmountPair(type, amount));
    }



    public void ForcePlayerInventoryClose()
    {
        player.CloseInventory();
    }
}
