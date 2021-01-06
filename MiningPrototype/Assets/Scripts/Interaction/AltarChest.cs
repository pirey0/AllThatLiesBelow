using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarChest : InventoryOwner
{
    [SerializeField] SpriteAnimator animator;
    [SerializeField] SpriteAnimation open, close;

    public override void OpenInventory()
    {
        animator.Play(open);
        base.OpenInventory();
    }

    public override void CloseInventory()
    {
        animator.Play(close);
        base.CloseInventory();
    }
}
