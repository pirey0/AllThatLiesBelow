using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemDrops : ScriptableObject
{
    [SerializeField] private ItemDrop[] drops;
    public ItemAmountPair GetRandomDrop()
    {
        List<ItemAmountPair> pairs = new List<ItemAmountPair>();

        foreach (ItemDrop drop in drops)
        {
            for (int i = 0; i < drop.DropRate; i++)
            {
                pairs.Add(drop.Drop);
            }
        }

        return (pairs[Random.Range(0, pairs.Count)]);
    }
}

[System.Serializable]
public class ItemDrop
{
    public ItemAmountPair Drop;
    public int DropRate;
}
