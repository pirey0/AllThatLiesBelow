using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShortcutVisualizer : MonoBehaviour
{
    [SerializeField] ShortcutHandler shortcutHandler;
    [SerializeField] ShortcutVisual prefab;

    [Zenject.Inject] PlayerInteractionHandler playerInteractionHandler;
    [Zenject.Inject] PlayerInventoryOpener playerInventoryOpener;

    Dictionary<ItemType, ShortcutVisual> visuals = new Dictionary<ItemType, ShortcutVisual>();

    private void OnEnable()
    {
        playerInteractionHandler.Inventory.InventoryChanged += UpdateShortcuts;
    }

    private void OnDisable()
    {
        playerInteractionHandler.Inventory.InventoryChanged -= UpdateShortcuts;
    }

    private void Start()
    {
        foreach (ItemAmountPair itemAmountPair in playerInteractionHandler.Inventory.GetContent())
        {
            UpdateShortcuts(true, itemAmountPair, false);
        }
    }

    private void UpdateShortcuts(bool add, ItemAmountPair element, bool playsound)
    {
        ItemType type = element.type;


        if (ItemsData.GetItemInfo(type).Shortcut == KeyCode.None)
            return;

        if (add)
        {
            if (!visuals.ContainsKey(type))
                visuals.Add(type, CreateNewVisuals(type));
        }
        else
        {
            if (!playerInteractionHandler.Inventory.Contains(new ItemAmountPair(type, 1)) && visuals.ContainsKey(type))
            {
                Destroy(visuals[type].gameObject);
                visuals.Remove(type);
            }
        }
    }

    private ShortcutVisual CreateNewVisuals(ItemType type)
    {
        var info = ItemsData.GetItemInfo(type);
        ShortcutVisual shortcutVisual = Instantiate(prefab, playerInventoryOpener.ShortcutParent);
        shortcutVisual.Init(info.DisplaySprite, info.Shortcut);
        return shortcutVisual;
    }
}
