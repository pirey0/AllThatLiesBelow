using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutHandler : MonoBehaviour
{
    [SerializeField] InventoryOwner inventory;

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
            if (Input.GetKeyDown(item.Key))
            {
                OnShortcutDown(item.Value);
            }
            else if (Input.GetKeyUp(item.Key))
            {
                OnShortcutUp(item.Value);
            }
        }
    }

    private void OnShortcutDown(ItemType value)
    {
        if(currentShortcut != ItemType.None)
        {
            OnShortcutUp(currentShortcut);
        }

        currentShortcut = value;
        Debug.Log("Pressed shortcut for " + value);
    }

    private void OnShortcutUp(ItemType value)
    {
        if(currentShortcut == value)
        {
            Debug.Log("Released shortcut for " + value);
        }
    }
}
