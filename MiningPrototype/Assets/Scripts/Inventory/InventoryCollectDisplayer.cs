using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayer : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    private void Start()
    {
        InventoryManager.Instance.PlayerCollected += OnPlayerCollected;
    }

    private void OnPlayerCollected(ItemAmountPair obj)
    {
        var go = Instantiate(prefab, transform);
        go.GetComponent<InventoryCollectDisplayElement>().SetText("+ " + obj.amount + " " + obj.type.ToString());
    }

}
