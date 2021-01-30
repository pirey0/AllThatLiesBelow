using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShortcutVisualizer : StateListenerBehaviour
{
    [SerializeField] ShortcutVisual prefab;

    [Zenject.Inject] PlayerManager playerManager;
    [Zenject.Inject] PlayerInventoryOpener playerInventoryOpener;

    Dictionary<ItemType, ShortcutVisual> visuals = new Dictionary<ItemType, ShortcutVisual>();

    protected override void OnRealStart()
    {
        playerManager.GetPlayerInventory().InventoryChanged += UpdateShortcuts;

        foreach (ItemAmountPair itemAmountPair in playerManager.GetPlayerInventory().GetContent())
        {
            UpdateShortcuts(true, itemAmountPair, false);
        }
    }

    private void OnDestroy()
    {
        if (playerManager != null)
            playerManager.GetPlayerInventory().InventoryChanged -= UpdateShortcuts;
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
            if (!playerManager.GetPlayerInventory().Contains(new ItemAmountPair(type, 1)) && visuals.ContainsKey(type))
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