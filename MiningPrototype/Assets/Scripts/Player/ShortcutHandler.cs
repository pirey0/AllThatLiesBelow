using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutHandler : MonoBehaviour
{
    [SerializeField] InventoryOwner inventory;

    [Zenject.Inject] ItemPlacingHandler itemPlacingHandler;
    [Zenject.Inject] CameraController cameraController;

    Dictionary<KeyCode, ItemType> shortcutDict;

    ItemType currentShortcut = ItemType.None;

    private void Start()
    {
        shortcutDict = new Dictionary<KeyCode, ItemType>();
        foreach (var item in System.Enum.GetValues(typeof(ItemType)))
        {
            var type = ItemsData.GetItemInfo((ItemType)item);
            if (type.Shortcut != KeyCode.None)
            {
                shortcutDict.Add(type.Shortcut, (ItemType)item);
            }
        }
    }

    private void Update()
    {
        foreach (var item in shortcutDict)
        {
            if (Input.GetKeyDown(item.Key) && inventory.Inventory.Contains(ItemAmountPair.One(item.Value)))
            {
                OnShortcutDown(item.Value);
            }
            else if (Input.GetKeyUp(item.Key))
            {
                OnShortcutUp(item.Value);
            }
        }

        if (currentShortcut != ItemType.None)
        {
            var mousePos = Util.MouseToWorld(cameraController.Camera);
            itemPlacingHandler.UpdatePosition(mousePos);
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            {
                itemPlacingHandler.TryPlace(currentShortcut, mousePos);
            }
            
        }
    }

    private void OnShortcutDown(ItemType value)
    {
        if (currentShortcut != ItemType.None)
        {
            OnShortcutUp(currentShortcut);
        }

        currentShortcut = value;
        itemPlacingHandler.Show(new ItemAmountPair(currentShortcut, 1), inventory.Inventory, OnCancel);
        Debug.Log("Pressed shortcut for " + value);
    }

    private void OnCancel()
    {
        Debug.Log("Cancelled shortcut press for " + currentShortcut);
        currentShortcut = ItemType.None;
    }

    private void OnShortcutUp(ItemType value)
    {
        if (currentShortcut == value)
        {
            Debug.Log("Released shortcut for " + value);
            itemPlacingHandler.Remove();
            currentShortcut = ItemType.None;
        }
    }
}
