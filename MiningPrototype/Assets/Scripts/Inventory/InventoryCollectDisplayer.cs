using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayer : MonoBehaviour
{
    [SerializeField] Image backdrop;
    [SerializeField] InventoryCollectDisplayElement prefab;
    [SerializeField] AudioSource onCollectSound;

    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] InventoryManager inventoryManager;

    List<GameObject> displayObjects = new List<GameObject>();
    private void Start()
    {
        inventoryManager.PlayerCollected += OnPlayerCollected;
    }

    private void OnDestroy()
    {
        inventoryManager.PlayerCollected -= OnPlayerCollected;
    }

    private void FixedUpdate()
    {
        float targetOpacity = 0;//transform.childCount / 3f;
        backdrop.color = new Color(0, 0, 0, Mathf.MoveTowards(backdrop.color.a, targetOpacity, 0.01f));
    }

    private void OnPlayerCollected(ItemAmountPair obj)
    {
        for (int i = displayObjects.Count - 1; i >= 0; i--)
        {
            if (displayObjects[i] == null)
                displayObjects.RemoveAt(i);
        }

        Vector3 position = Util.MouseToWorld(cameraController.Camera) + Vector3.down * 0.5f * displayObjects.Count;
        var go = Instantiate(prefab, position, Quaternion.identity, transform); //safe no injection needed
        go.SetItem(obj);
        displayObjects.Add(go.gameObject);

        if (onCollectSound != null)
            onCollectSound.Play();
    }

}
