using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayer : MonoBehaviour
{
    [SerializeField] InventoryCollectDisplayElement prefab;

    private void Start()
    {
        InventoryManager.Instance.PlayerCollected += OnPlayerCollected;
    }

    private void OnPlayerCollected(ItemAmountPair obj)
    {
        var go = Instantiate(prefab, transform);
        go.SetItem(obj);
    }

}
