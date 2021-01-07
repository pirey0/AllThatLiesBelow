using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryOwnerDropBased : InventoryOwner
{
    [SerializeField] ItemDrops drops;
    bool touched = false;

    public override void OpenInventory()
    {
        if (!touched)
        {
            touched = true;

            if (Inventory.Count <= 0)
            {
                Inventory.Add(drops.GetRandomDrop());
            }
        }
        base.OpenInventory();
    }
}
