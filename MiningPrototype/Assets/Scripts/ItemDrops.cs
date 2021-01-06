using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemDrops : ScriptableObject
{
    [SerializeField] private ItemDrop[] drops;
    public ItemAmountPair GetRandomDrop()
    {
        if (drops == null || drops.Length == 0)
            return ItemAmountPair.Nothing;
        
        int i = Random.Range(0, GetDropSum());
        foreach (var d in drops)
        {
            if(d.DropRate > i)
            {
                return d.Drop;
            }
            else
            {
                i -= d.DropRate;
            }
        }
        throw new System.Exception();
    }

    private int GetDropSum()
    {
        int s = 0;
        foreach (var d in drops)
        {
            s += d.DropRate;
        }

        return s;
    }
}

[System.Serializable]
public class ItemDrop
{
    public ItemAmountPair Drop;
    public int DropRate;
}
