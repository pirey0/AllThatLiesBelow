using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NewOrderCrateSpawner : MonoBehaviour
{
    [SerializeField] List<ItemAmountPair> testOrder;
    [SerializeField] List<CrateInfo> crateInfos;
    [SerializeField] Vector3 boxSpawnLocation;

    private void Start()
    {
        if (crateInfos.Count == 0)
        {
            Debug.LogError("No crates defined.");
        }

        crateInfos.Sort((x, y) => x.MaxCapacity - y.MaxCapacity);
    }

    [Button]
    public void SpawnTestOrder()
    {
        SpawnOrder(testOrder);
    }

    public void SpawnOrder(List<ItemAmountPair> orderSource)
    {
        var order = new List<ItemAmountPair>(orderSource);

        for (int i = 0; i < order.Count; i++)
        {
            var item = order[i];
            Crate cratePrefab = GetRandomCrateThatFits(item.GetTotalWeight());

            if (cratePrefab == null)
            {
                var split = SplitToFit(item, crateInfos[crateInfos.Count - 1].MaxCapacity);
                Debug.Log("Splitting");

                foreach (var s in split)
                {
                    order.Add(s);
                }
            }
            else
            {
                Crate newCrate = Instantiate(cratePrefab, boxSpawnLocation, Quaternion.identity);
                newCrate.Pack(item);
            }
        }
    }

    private List<ItemAmountPair> SplitToFit(ItemAmountPair pair, int maxWeight)
    {
        List<ItemAmountPair> result = new List<ItemAmountPair>();

        while (pair.GetTotalWeight() > maxWeight)
        {
            int amount = maxWeight / ItemsData.GetItemInfo(pair.type).Weight;

            result.Add(new ItemAmountPair(pair.type, amount));
            pair.amount -= amount;
        }
        if (pair.amount > 0)
            result.Add(pair);

        return result;
    }

    private Crate GetRandomCrateThatFits(int weight)
    {
        List<Crate> availableCrates = new List<Crate>();
        int currentSize = -1;

        foreach (var info in crateInfos)
        {
            if (info.MaxCapacity >= weight && (info.MaxCapacity == currentSize || currentSize < 0))
            {
                availableCrates.Add(info.CratePrefab);
                currentSize = info.MaxCapacity;
            }
        }

        if (availableCrates.Count > 0)
        {
            return availableCrates[Random.Range(0, availableCrates.Count)];
        }
        else
        {
            return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boxSpawnLocation, Vector3.one);
    }
}

[System.Serializable]
public struct CrateInfo
{
    public Crate CratePrefab;
    public int MaxCapacity;
}