using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, IMinableNonGrid
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PassiveSpriteAnimator overlayAnimator;
    [SerializeField] float damageMultiplier = 1;

    [SerializeField] ItemAmountPair contains;
    [SerializeField] GameObject destroyEffects;

    float damage = 0;

    public void Damage(float v)
    {
        float d = v * (damageMultiplier/10f);
        Debug.LogWarning("damage: " + d + " (" + damage + ")");
        overlayAnimator.ActiveUpdate(d);
        damage += d;

        if (damage >= 1)
            Destroyed();
    }

    private void Destroyed()
    {
        if (contains != null && contains.amount > 0)
            InventoryManager.PlayerCollects(contains.type, contains.amount);

        Instantiate(destroyEffects, GetPosition(), Quaternion.identity);

        Destroy(gameObject);
    }

    public Vector2 GetPosition()
    {
        return transform.position + (spriteRenderer.size.y/2) * Vector3.up;
    }

    public void MouseEnter()
    {
        //
    }

    public void MouseLeave()
    {
        //
    }
}
