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
    Dictionary<ItemType, int> cost = new Dictionary<ItemType, int>();
    [SerializeField] TMP_Text costText;

    [SerializeField] Button buyButton;
    [SerializeField] Sprite checkmark, x;

    [SerializeField] TMP_Text costElementPrefab;
    [SerializeField] Transform costGrid;

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
        //clear cost display
        foreach (Transform child in costGrid)
        {
            Destroy(child.gameObject);
        }

        cost.Clear();

        //new cost fetching after implementation
        //foreach (KeyValuePair<ItemType, int> item in orderedElementsWithAmounts)
        //{
        //    ItemAmountPair price = GetPriceFor(item.Key, item.Value);
        //    cost[price.type] += price.amount;
        //}

        //calculate the new cost
        float amount = 0;

        foreach (int i in orderedElementsWithAmounts.Values)
            amount += i;

        if (amount <= 0)
            return;

        cost[ItemType.Gold] = Mathf.FloorToInt((1f / 3f) * amount);
        cost[ItemType.Copper] = Mathf.FloorToInt(((2f/3f) * amount * 2f) - cost[ItemType.Gold] * 2f);

        //create cost visualization
        foreach (KeyValuePair<ItemType, int> costElement in cost)
        {
            if (costElement.Value > 0)
            {
                TMP_Text costText = Instantiate(costElementPrefab, costGrid);
                Image costIcon = costText.GetComponentInChildren<Image>();

                ItemInfo costInfo = ItemsData.GetItemInfo(costElement.Key);

                //costText.text = costInfo.DisplayName;
                costText.text = costElement.Value.ToString() + "x";
                costIcon.sprite = costInfo.DisplaySprite;
            }
        }


        //update text and checkbox
        string text = "check this box to buy.";

        if (!CheckIfCanBuy(amount, cost))
            text += "\n- too expensive -";

        costText.text = text;

        
    }

    private bool CheckIfCanBuy(float amountOfElementsOrdered, Dictionary<ItemType, int> costsToOrder)
    {
        if (amountOfElementsOrdered == 0)
        {
            CanBuy(false);
            return true;
        }
        else
        {
            if (costsToOrder[ItemType.Gold] > 2)
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
