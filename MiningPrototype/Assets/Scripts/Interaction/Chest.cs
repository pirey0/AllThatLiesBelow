using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InventoryOwner, IHoverable, IDropReceiver
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite chestOpen, chestClosed;
    [SerializeField] Material defaultMat, outlineMat;

    public override void OpenInventory()
    {
        base.OpenInventory();
        spriteRenderer.sprite = chestOpen;
    }

    public override void CloseInventory()
    {
        base.CloseInventory();
        spriteRenderer.sprite = chestClosed;
    }

    public void HoverEnter()
    {
        spriteRenderer.material = outlineMat;
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f);
}

    public void HoverExit()
    {
        spriteRenderer.material = defaultMat;
        spriteRenderer.color = Color.white;
    }

    public void HoverUpdate()
    {
        //
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return true;
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        //
    }

    public void EndHover()
    {
        //
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair, Inventory origin)
    {
        if (origin.Contains(pair) && origin.TryRemove(pair))
            Inventory.Add(pair);
    }
}
