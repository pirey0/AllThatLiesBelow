using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewOrderUpgradeUIElement : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Text text;
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] Color color_notSelected, color_selected;
    [Zenject.Inject] ProgressionHandler progressionHandler;

    bool selected;
    string displayName;
    NewOrderVisualizer order;

    private void Start()
    {
        UpdateText();
        order = GetComponentInParent<NewOrderVisualizer>();
        if (progressionHandler.IsMaxUpgradeLevel(upgradeType))
            Destroy(gameObject);
        displayName = progressionHandler.GetDisplayNameForUpgrade(upgradeType);
        UpdateText();
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
        order?.TryAddUpgrade(upgradeType);
        UpdateText();
    }

    public void Decrease()
    {
        if (selected == false)
            return;

        selected = false;
        order?.TryRemoveUpgrade(upgradeType);
        UpdateText();
    }
}
