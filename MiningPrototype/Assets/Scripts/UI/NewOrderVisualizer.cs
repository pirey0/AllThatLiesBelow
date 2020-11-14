using NaughtyAttributes.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewOrderVisualizer : MonoBehaviour
{
    Dictionary<ItemType, int> orderedElementsWithAmounts = new Dictionary<ItemType, int>();
    [SerializeField] TMP_Text costText;

    [SerializeField] Button buyButton;
    [SerializeField] Sprite checkmark, x;

    private void Start()
    {
        UpdateCost();
    }

    public void UpdateAmount(ItemType itemType, int amount)
    {
        orderedElementsWithAmounts[itemType] = amount;
        UpdateCost();
    }

    private void UpdateCost()
    {
        float amount = 0;

        foreach (int i in orderedElementsWithAmounts.Values)
            amount += i;

        int gold = Mathf.FloorToInt((1f / 3f) * amount);
        int copper = Mathf.FloorToInt(((2f/3f) * amount * 2f) - gold * 2f);

        string cost = copper + " copper";

        if (amount == 0)
            cost = "please select what you want to order.";

        if (gold > 0)
            cost += " & " + gold + " gold";

        if (!CheckIfCanBuy(amount, gold))
            cost += "\n- too expensive -";

        costText.text = cost;

        
    }

    private bool CheckIfCanBuy(float amountOfElementsOrdered,int amountOfGold)
    {
        if (amountOfElementsOrdered == 0)
        {
            CanBuy(false);
            return true;
        }
        else
        {
            if (amountOfGold > 2)
            {
                CanBuy(false);
                return false;
            }
            else
            {
                CanBuy(true);
                return true;
            }
        }
    }
    private void CanBuy(bool canBuy)
    {
        (buyButton.targetGraphic as Image).sprite = canBuy ? checkmark : x;
        buyButton.interactable = canBuy;
    }

    public void Cancel ()
    {
        Destroy(gameObject);
    }

    public void Submit()
    {
        List<ItemAmountPair> itemAmountPairs = new List<ItemAmountPair>();

        foreach (KeyValuePair<ItemType, int> i in orderedElementsWithAmounts)
            itemAmountPairs.Add(new ItemAmountPair(i.Key, i.Value));

        ProgressionHandler.Instance.AddOrderForNextDay(itemAmountPairs);

        Destroy(gameObject);
    }
}
