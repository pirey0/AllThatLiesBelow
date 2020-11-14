using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineableObject : MonoBehaviour, IMinableNonGrid
{
    [SerializeField] PassiveSpriteAnimator overlayAnimator;
    [SerializeField] float damageMultiplier = 1;

    [SerializeField] protected ItemAmountPair contains;
    [SerializeField] GameObject destroyEffects;

    float damage = 0;

    public void Damage(float v)
    {
        float d = v * (damageMultiplier / 10f);
        overlayAnimator.ActiveUpdate(d);
        damage += d;

        if (damage >= 1)
            Destroyed();
    }

    private void Destroyed()
    {
        if (contains != null && contains.amount > 0)
            InventoryManager.PlayerCollects(contains.type, contains.amount);

        if (destroyEffects != null)
            Instantiate(destroyEffects, GetPosition(), Quaternion.identity);

        Destroy(gameObject);
    }

    public virtual Vector2 GetPosition()
    {
        return transform.position;
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
