using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NewOrderCrateSpawner : MonoBehaviour
{
    [SerializeField] List<ItemAmountPair> toSpawnForTesting;
    [SerializeField] List<Crate> boxPrefabs;
    [SerializeField] Vector3 boxSpawnLocation;

    [Button]
    public void SpawnTestOrder()
    {
        SpawnOrder(toSpawnForTesting);
    }

    public void SpawnOrder(List<ItemAmountPair> itemsToSpawnInBoxes)
    {
        foreach (ItemAmountPair item in itemsToSpawnInBoxes)
        {
            Crate newCrate = Instantiate(boxPrefabs[Random.Range(0,boxPrefabs.Count)],boxSpawnLocation,Quaternion.identity);
            newCrate.Pack(item);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boxSpawnLocation,Vector3.one);
    }
}
