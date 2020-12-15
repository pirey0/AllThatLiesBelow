using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NewOrderCrateSpawner : StateListenerBehaviour
{
    [SerializeField] List<ItemAmountPair> testOrder;
    [SerializeField] List<CrateInfo> crateInfos;
    [SerializeField] GameObject cratePrefab;

    [Zenject.Inject] PrefabFactory prefabFactory;

    LocationIndicator spawnLoc;

    private void Start()
    {
        if (crateInfos.Count == 0)
        {
            Debug.LogError("No crates defined.");
        }

        crateInfos.Sort((x, y) => x.MaxCapacity - y.MaxCapacity);
    }


    protected override void OnRealStart()
    {
        if (spawnLoc == null)
            spawnLoc = LocationIndicator.Find(IndicatorType.OrderSpawn);
    }


    [Button]
    public void SpawnTestOrder()
    {
        SpawnOrder(testOrder);
    }

    public void SpawnOrder(List<ItemAmountPair> orderSource)
    {
        if(spawnLoc == null)
            spawnLoc = LocationIndicator.Find(IndicatorType.OrderSpawn);

        var order = new List<ItemAmountPair>(orderSource);
        order.Sort((x, y) => y.GetTotalWeight() - x.GetTotalWeight());

        for (int i = 0; i < order.Count; i++)
        {
            var item = order[i];
            CrateType crateType = GetRandomCrateThatFits(item.GetTotalWeight());

            if (crateType < 0)
            {
                var split = SplitToFit(item, crateInfos[crateInfos.Count - 1].MaxCapacity);
                Debug.Log("Splitting");

                foreach (var s in split)
                {
                    order.Insert(i + 1, s);
                }
            }
            else
            {
                if(spawnLoc == null)
                {
                    Debug.LogError("No spawn location for crateSpawner.");
                }
                else
                {
                    Vector3 position = spawnLoc.transform.position + new Vector3(0, i * 2);
                    Crate newCrate = prefabFactory.Create(cratePrefab, position, Quaternion.identity).GetComponent<Crate>();
                    newCrate.SetCrateType(crateType);
                    newCrate.Pack(item);
                }
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

    private CrateType GetRandomCrateThatFits(int weight)
    {
        List<CrateType> availableCrates = new List<CrateType>();
        int currentSize = -1;

        foreach (var info in crateInfos)
        {
            if (info.MaxCapacity >= weight && (info.MaxCapacity == currentSize || currentSize < 0))
            {
                availableCrates.Add(info.CrateType);
                currentSize = info.MaxCapacity;
            }
        }

        if (availableCrates.Count > 0)
        {
            return availableCrates[Random.Range(0, availableCrates.Count)];
        }
        else
        {
            return (CrateType)(-1);
        }
    }


}

[System.Serializable]
public struct CrateInfo
{
    public CrateType CrateType;
    public int MaxCapacity;
}