using NaughtyAttributes.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class NewOrderVisualizer : ReadableItemVisualizer
{
    [SerializeField] TMP_Text costText;
    [SerializeField] Button buyButton;
    [SerializeField] Sprite checkmark, x;
    [SerializeField] TMP_Text costElementPrefab;
    [SerializeField] Transform costGrid;
    [SerializeField] GameObject leftclick, rightclick;
    [SerializeField] AudioSource writingSounds;

    [Inject] ProgressionHandler progressionHandler;
    [Inject] ShopPricesParser shopPricesParser;
    [Inject] InventoryManager inventoryManager;

    Dictionary<ItemType, int> orderedElementsWithAmounts = new Dictionary<ItemType, int>();
    List<UpgradeType> orderedUpgrades = new List<UpgradeType>();
    Dictionary<ItemType, int> cost = new Dictionary<ItemType, int>();
    Action<Order> OnFinish;
    Action OnAbort;

    protected override void Start()
    {
        StartCoroutine(ScaleCoroutine(scaleUp: true));
        UpdateCost();
        UpdateTutorialDisplays();
    }

    //Hand over the methods to call for fimish and abort from the desk
    public void Handshake(Action<Order> onFinish,Action onAbort)
    {
        OnFinish = onFinish;
        OnAbort = onAbort;
    }

    public void UpdateAmount(ItemType itemType, int amount, bool increased)
    {
        if (increased)
            progressionHandler.NotifyPassedTutorialFor("NewOrderLeftClick");
        else
            progressionHandler.NotifyPassedTutorialFor("NewOrderRightClick");

        UpdateTutorialDisplays();

        if (writingSounds != null)
        {
            writingSounds.pitch = UnityEngine.Random.Range(0.8f,1.2f);
            writingSounds.Play();
        }

        if (amount <= 0)
            orderedElementsWithAmounts.Remove(itemType);
        else
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
        foreach (KeyValuePair<ItemType, int> item in orderedElementsWithAmounts)
        {
            ItemAmountPair price = shopPricesParser.GetPriceFor(item.Key, item.Value);

            if (cost.ContainsKey(price.type))
                cost[price.type] += price.amount;
            else
                cost.Add(price.type, price.amount);
        }

        //create cost visualization
        foreach (KeyValuePair<ItemType, int> costElement in cost)
        {
            if (costElement.Value > 0)
            {
                TMP_Text costText = Instantiate(costElementPrefab, costGrid); //safe no injection needed
                Image costIcon = costText.GetComponentInChildren<Image>();

                ItemInfo costInfo = ItemsData.GetItemInfo(costElement.Key);

                //costText.text = costInfo.DisplayName;
                costText.text = costElement.Value.ToString() + "x";
                costIcon.sprite = costInfo.DisplaySprite;
            }
        }


        //update text and checkbox
        string text = "check this box to finish the order, put it in the postbox to order.";

        if (!CheckIfCanBuy(cost))
            text = "- too expensive -";

        costText.text = text;


    }

    private bool CheckIfCanBuy(Dictionary<ItemType, int> costsToOrder)
    {
        foreach (KeyValuePair<ItemType, int> i in costsToOrder)
        {
            if (!inventoryManager.PlayerHas(i.Key, i.Value))
                return SetCanBuy(false);
        }

        return SetCanBuy(true);
    }
    private bool SetCanBuy(bool canBuy)
    {
        (buyButton.targetGraphic as Image).sprite = canBuy ? checkmark : x;
        buyButton.interactable = canBuy;

        return canBuy;
    }

    public void Cancel()
    {
        OnAbort?.Invoke();
        Hide();
    }

    public void Submit()
    {
        List<ItemAmountPair> itemAmountPairs = new List<ItemAmountPair>();
        foreach (KeyValuePair<ItemType, int> i in orderedElementsWithAmounts)
            itemAmountPairs.Add(new ItemAmountPair(i.Key, i.Value));

        OnFinish?.Invoke(new Order(cost, itemAmountPairs.ToArray(), orderedUpgrades.ToArray()));
        Hide();
    }

    private void UpdateTutorialDisplays()
    {
        leftclick.SetActive(progressionHandler.NeedsTutorialFor("NewOrderLeftClick"));
        rightclick.SetActive(progressionHandler.NeedsTutorialFor("NewOrderRightClick"));
    }
}
