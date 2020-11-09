using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotVisualizer : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Text amountDisplay;

    [SerializeField] ItemInformationStorage ItemInformationStorage;

    int amount;
    itemType type;

    public void Display(KeyValuePair<itemType, int> pair)
    {
        amount = pair.Value;
        type = pair.Key;

        if (icon != null)
            icon.sprite = ItemInformationStorage.GetSpriteByItemType(type);

        if (amountDisplay != null)
            amountDisplay.text = amount.ToString();
    }
}
