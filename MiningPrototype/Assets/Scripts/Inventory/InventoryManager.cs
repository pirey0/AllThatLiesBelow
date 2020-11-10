using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryManager
{
    private static InventoryManager instance;
    public static InventoryManager Instance { get => GetInstance(); }

    private Inventory playerInventory;
    private Inventory otherInventory;
    bool playerInventoryOpen;

    public event System.Action<ItemAmountPair> PlayerCollected;
    public event System.Action<ItemAmountPair> AttemptedTransfer;

    private static InventoryManager GetInstance()
    {
        if (instance == null)
        {
            instance = new InventoryManager();
            var player = GameObject.FindObjectOfType<PlayerController>();
            if (player != null)
            {
                instance.playerInventory = player.Inventory;
            }
            else
            {
                Debug.LogError("No PlayerController found.");
            }
        }
        return instance;
    }

    public static void NotifyInventoryOpen(IInventoryOwner owner)
    {
        if (owner.Inventory == Instance.playerInventory)
        {
            Instance.playerInventoryOpen = true;
        }
        else
        {
            Instance.otherInventory = owner.Inventory;
        }
    }

    public static void NotifyInventoryClosed(IInventoryOwner owner)
    {
        if (owner.Inventory == Instance.playerInventory)
        {
            Instance.playerInventoryOpen = false;
        }
        else if (owner.Inventory == Instance.otherInventory)
        {
            Instance.otherInventory = null;
        }
    }

    public static void PlayerCollects(ItemType itemType, int amount)
    {
        if (Instance.playerInventory != null)
            Instance.playerInventory.Add(itemType, amount);

        Instance.PlayerCollected?.Invoke(new ItemAmountPair(itemType, amount));
    }

    public static bool PlayerTryPay(ItemType itemType, int amount)
    {
        return Instance.playerInventory.TryRemove(new ItemAmountPair(itemType, amount));
    }

    public static void TryMove(Inventory inventory, int stackIndex)
    {
        //player to other
        if (inventory == Instance.playerInventory)
        {
            if (Instance.otherInventory != null)
            {
                if (Instance.playerInventoryOpen)
                {
                    var result = Instance.playerInventory.RemoveStack(stackIndex);
                    if (result != null)
                        Instance.otherInventory.Add(result);
                    Debug.Log("InventoryResult: Moved to other");
                }
                else
                {
                    Debug.Log("InventoryError: Player inventory closed");
                }
            }
            else
            {
                Debug.Log("InventoryResult: Other inventory closed, attempting payment");
                Instance.AttemptedTransfer?.Invoke(inventory[stackIndex]);
            }
        }
        //other to player
        else if (inventory == Instance.otherInventory)
        {
            if (Instance.playerInventoryOpen)
            {
                var result = Instance.otherInventory.RemoveStack(stackIndex);
                if (result != null)
                    Instance.playerInventory.Add(result);

                Debug.Log("InventoryResult: Moved to player");
            }
            else
            {
                Debug.Log("InventoryError: Player inventory closed");
            }
        }
        else
        {
            Debug.Log("InventoryError: Moving failed unknown error");
        }
    }
}
