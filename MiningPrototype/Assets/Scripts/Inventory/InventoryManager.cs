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

    public static void TryMove(Inventory inventory, int stackIndex)
    {
        //player to other
        if (inventory == Instance.playerInventory)
        {
            if ( Instance.otherInventory != null)
            {
                if (Instance.playerInventoryOpen)
                {
                    var result = Instance.playerInventory.Remove(stackIndex);
                    if (result != null)
                        Instance.otherInventory.Add(result);
                    Debug.Log("Moved to other");
                }
                else
                {
                    Debug.LogError("InventoryError: Player inventory closed");
                }
            }
            else
            {
                Debug.LogError("InventoryError: Other inventory closed");
            }
        }
        //other to player
        else if (inventory == Instance.otherInventory)
        {
            if (Instance.playerInventoryOpen)
            {
                var result = Instance.otherInventory.Remove(stackIndex);
                if (result != null)
                    Instance.playerInventory.Add(result);

                Debug.Log("Moved to player");
            }
            else
            {
                Debug.LogError("InventoryError: Player inventory closed");
            }
        }
        else
        {
            Debug.LogError("InventoryError: Moving failed unknown error");
        }
    }
}
