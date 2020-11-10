using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayer : MonoBehaviour
{
    [SerializeField] Image backdrop;
    [SerializeField] InventoryCollectDisplayElement prefab;

    private void Start()
    {
        InventoryManager.Instance.PlayerCollected += OnPlayerCollected;
    }

    private void FixedUpdate()
    {
        float targetOpacity = transform.childCount / 3f;
        backdrop.color = new Color(0,0,0,Mathf.MoveTowards(backdrop.color.a,targetOpacity,0.01f));
    }

    private void OnPlayerCollected(ItemAmountPair obj)
    {
        var go = Instantiate(prefab, transform);
        go.SetItem(obj);
    }

}
