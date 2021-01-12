using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour, IDropReceiver
{
    [SerializeField] new ParticleSystem particleSystem;
    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        bool takes = false;
        var info = ItemsData.GetItemInfo(pair.type);
        if (info != null)
            takes = info.IsBurnable;
        return takes;
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
        if (origin.Contains(pair))
        {
            origin.TryRemove(pair);
            particleSystem.Emit(Mathf.Clamp(2*pair.amount,10,100));
        }
    }
}
