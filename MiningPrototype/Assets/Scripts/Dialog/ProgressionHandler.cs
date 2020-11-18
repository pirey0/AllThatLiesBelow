using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionHandler : Singleton<ProgressionHandler>, ISavable
{
    [ReadOnly] [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] string dialog;
    [SerializeField] string alreadyTradedDialog;
    [SerializeField] float speedPerBlessing, digSpeedPerBlessing;
    [SerializeField] NewOrderCrateSpawner newOrderCrateSpawner;

    List<string> aquiredList = new List<string>();
    List<ItemAmountPair> orderForNextDay = new List<ItemAmountPair>();
 
    private float speedMultiplyer = 1;
    private float digSpeedMultiplyer = 1;
    private int extraDrop = 1;
    private int day = 0;
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
        day++;
    }

    public void AddOrderForNextDay(List<ItemAmountPair> newOrder)
    {
        orderForNextDay.AddRange(newOrder);
    }

    public float GetPriceOf(string reward, string resource)
    {
        return SacrificePricesParser.GetPriceFor(reward, resource);
    }

    public SaveData ToSaveData()
    {
        ProgressionSaveData saveData = new ProgressionSaveData();
        saveData.GUID = GetSaveID();
        saveData.AquiredList = aquiredList;
        saveData.OrderForNextDay = orderForNextDay;
        saveData.SpeedMultiplyer = speedMultiplyer;
        saveData.DigSpeedMultiplyer = digSpeedMultiplyer;
        saveData.ExtraDrop = extraDrop;
        saveData.DailyPurchaseExaused = dailyPurchaseExaused;
        saveData.Day = day;

        return saveData;
    }

    public void Load(SaveData data)
    {
        if(data is ProgressionSaveData saveData)
        {
            aquiredList = saveData.AquiredList;
            orderForNextDay = saveData.OrderForNextDay;
            speedMultiplyer = saveData.SpeedMultiplyer;
            digSpeedMultiplyer = saveData.DigSpeedMultiplyer;
            extraDrop = saveData.ExtraDrop;
            dailyPurchaseExaused = saveData.DailyPurchaseExaused;
            day = saveData.Day;
        }
        else
        {
            Debug.LogError("Wrong SaveData type for ProgressionHandler");
        }
    }

    public string GetSaveID()
    {
        return saveID;
    }
}


[System.Serializable]
public class ProgressionSaveData : SaveData
{
    public List<string> AquiredList = new List<string>();
    public List<ItemAmountPair> OrderForNextDay = new List<ItemAmountPair>();

    public float SpeedMultiplyer = 1;
    public float DigSpeedMultiplyer = 1;
    public int ExtraDrop = 1;
    public int Day;
    public bool DailyPurchaseExaused;
}