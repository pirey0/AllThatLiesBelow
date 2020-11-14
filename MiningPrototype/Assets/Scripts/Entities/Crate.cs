using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MineableObject
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public void Pack(ItemAmountPair toPack)
    {
        contains = toPack;
    }
    public override Vector2 GetPosition()
    {
        if (overlayAnimator != null)
            return overlayAnimator.transform.position;
        else
            return transform.position + (spriteRenderer.size.y / 2) * transform.up;
    }
}
