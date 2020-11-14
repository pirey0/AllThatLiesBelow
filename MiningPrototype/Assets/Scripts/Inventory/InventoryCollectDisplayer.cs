using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayer : MonoBehaviour
{
    [SerializeField] Image backdrop;
    [SerializeField] InventoryCollectDisplayElement prefab;
    Camera main;

    private void Start()
    {
        InventoryManager.Instance.PlayerCollected += OnPlayerCollected;
        main = Camera.main;
    }

    private void FixedUpdate()
    {
        float targetOpacity = 0;//transform.childCount / 3f;
        backdrop.color = new Color(0,0,0,Mathf.MoveTowards(backdrop.color.a,targetOpacity,0.01f));
    }

    private void OnPlayerCollected(ItemAmountPair obj)
    {
        Vector3 position = Util.MouseToWorld(main);
        var go = Instantiate(prefab, position, Quaternion.identity, transform);
        go.SetItem(obj);
    }

}
