using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionHandler : Singleton<ProgressionHandler>
{
    [SerializeField] string dialog;
    [SerializeField] string alreadyTradedDialog;
    [SerializeField] float speedPerBlessing, digSpeedPerBlessing;

    List<string> aquiredList = new List<string>();
    List<ItemAmountPair> orderForNextDay = new List<ItemAmountPair>();

    [SerializeField] NewOrderCrateSpawner newOrderCrateSpawner;
 
    private float speedMultiplyer = 1;
    private float digSpeedMultiplyer = 1;
    private int extraDrop = 1;

    bool dailyPurchaseExaused;

    public float SpeedMultiplyer { get => speedMultiplyer; }
    public float DigSpeedMultiplyer { get => digSpeedMultiplyer; }
    public int ExtraDrop { get => extraDrop; }

    public IDialogSection GetCurrentAltarDialog()
    {
        var di = DialogParser.GetDialogFromName(dailyPurchaseExaused ? alreadyTradedDialog : dialog);

        return di;
    }

    public void Aquired(string topic)
    {
        Debug.Log(topic + " unlocked in the morning!");
        dailyPurchaseExaused = true;
        aquiredList.Add(topic);
    }

    public void StartNextDay()
    {
        //sacrifices
        dailyPurchaseExaused = false;

        foreach (var aquired in aquiredList)
        {

            Debug.Log("Aquired: " + aquired);
            switch (aquired)
            {
                case "Wealth":
                    extraDrop += 1;
                    break;
                case "Strength":
                    digSpeedMultiplyer *= digSpeedPerBlessing;
                    break;
                case "Speed":
                    speedMultiplyer *= speedPerBlessing;
                    break;
                default:
                    Debug.Log("Unimplemented aquired bonus: " + aquired);
                    break;

            }
        }

        aquiredList.Clear();

        //order
        newOrderCrateSpawner.SpawnOrder(orderForNextDay);
        orderForNextDay.Clear();
    }

    public void AddOrderForNextDay(List<ItemAmountPair> newOrder)
    {
        orderForNextDay.AddRange(newOrder);
    }

    public float GetPriceOf(string reward, string resource)
    {
        return PricesParser.GetPriceFor(reward, resource);
    }
}
