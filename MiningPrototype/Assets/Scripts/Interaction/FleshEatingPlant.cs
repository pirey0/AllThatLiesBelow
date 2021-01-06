using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshEatingPlant : BasicNonPersistantSavable
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation walkOver, snap;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (spriteAnimator.IsDone())
            spriteAnimator.Play(walkOver);
    }
}
