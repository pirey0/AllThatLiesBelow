using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineableObject : MirrorWorldFollower, IMinableNonGrid
{
    [SerializeField] protected PassiveSpriteAnimator overlayAnimator;
    [SerializeField] float damageMultiplier = 1;

    [SerializeField] protected ItemAmountPair contains;
    [SerializeField] GameObject destroyEffects;

    [Zenject.Inject] protected InventoryManager inventoryManager;


    float damage = 0;

    public virtual void Damage(float v)
    {
        if (Util.IsNullOrDestroyed(transform))
            return;

        float d = v * (damageMultiplier / 10f);
        overlayAnimator?.ActiveUpdate(d);
        damage += d;

        if (damage >= 1)
            Destroyed();
    }

    protected virtual void Destroyed()
    {
        if (!contains.IsNull() && contains.amount > 0)
        {
            inventoryManager.PlayerCollects(contains.type, contains.amount);
        }

        if (destroyEffects != null)
            Instantiate(destroyEffects, GetPosition(), Quaternion.identity); //Safe no injection

        Destroy(gameObject);
    }

    public virtual Vector2 GetPosition()
    {
        if (Util.IsNullOrDestroyed(transform))
            return Vector3.zero;

        return transform.position;
    }

    //NOT USED AT ALL
    public void MouseLeave()
    {
    }

    public void MouseEnter()
    {
    }
}
