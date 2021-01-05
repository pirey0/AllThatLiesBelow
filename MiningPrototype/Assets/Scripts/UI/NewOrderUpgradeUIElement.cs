using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewOrderUpgradeUIElement : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Text text;
    [SerializeField] ItemType upgradeType;
    [SerializeField] Color color_notSelected, color_selected;
    [Zenject.Inject] ProgressionHandler progressionHandler;

    bool selected;
    string displayName;
    NewOrderVisualizer order;

    private void Start()
    {
        var info = ItemsData.GetItemInfo(upgradeType);

        if (!ProgressionRequirementsAreFullfilled(info.RequiredLevel))
            Destroy(gameObject);

        UpdateText();
        order = GetComponentInParent<NewOrderVisualizer>();
        displayName = info.DisplayName;
        UpdateText();
    }

    private bool ProgressionRequirementsAreFullfilled(int requiredLevel)
    {
        switch (upgradeType)
        {
            case ItemType.IronPickaxe:
            case ItemType.SteelPickaxe:
            case ItemType.DiamondPickaxe:
                return requiredLevel == progressionHandler.PickaxeLevel;
        }

        return false;
    }

    private void UpdateText()
    {
        text.text = (selected?"1":"0") + " x " + displayName;
        text.color = (selected) ? color_selected : color_notSelected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            Decrease();
    }

    public void Increase()
    {
        if (selected == true)
            return;

        selected = true;
        order?.UpdateAmount(upgradeType, 1, increased: true);
        UpdateText();
    }

    public void Decrease()
    {
        if (selected == false)
            return;

        selected = false;
        order?.UpdateAmount(upgradeType, 0, increased: false);
        UpdateText();
    }
}
