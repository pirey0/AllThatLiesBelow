using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionHandler : MonoBehaviour, ISavable
{
    [ReadOnly] [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] float rewardASpeedMultiplyer, rewardDigSpeedMultiplyer, rewardStrengthMultiplyer, rewardJumpMultiplyer;
    [SerializeField] NewOrderCrateSpawner newOrderCrateSpawner;
    [SerializeField] int startingLetterID = 100;
    [SerializeField] List<ItemAmountPair> startingItems;
    [SerializeField] string debugRewardToGet;

    [Zenject.Inject] OverworldEffectHandler overworldEffectHandler;

    
    List<(string,ItemAmountPair)> aquiredList = new List<(string, ItemAmountPair)>();
    Dictionary<int, List<ItemAmountPair>> ordersForNextDay = new Dictionary<int, List<ItemAmountPair>>();
    private int day = 0;
    bool dailySacrificeExaused;
    int sacrificeProgressionLevel = 1;

    //sacrifice rewards
    private float speedMultiplyer = 1;
    private float digSpeedMultiplyer = 1;
    private float strengthMultiplyer = 1;
    private float jumpMultiplyer = 1;
    private bool isSpring = false;
    private bool isMidas = false;
    private bool hasLove = false;
    private bool hasWon = false;
    private bool hasWayOut = false;
    private bool isFree = false;

    //sacriifce consequences

    public bool showNewOrderLeftClickInfo = true, showNewOrderRightClickInfo = true;

    //postbox and letters
    Postbox postbox;
    int lastLetterID = -1;
    bool wifeRecievedLetter = false;
    LetterProgressionState letterProgressionState = LetterProgressionState.RecievedDay;

    public float SpeedMultiplyer { get => speedMultiplyer; }
    public float DigSpeedMultiplyer { get => digSpeedMultiplyer; }
    public bool DailySacrificeExpired {  get => dailySacrificeExaused; }
    public int SacrificeProgressionLevel { get => sacrificeProgressionLevel; }

    private void OnEnable()
    {
        GameState.Instance.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        GameState.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState.State state)
    {
        if (state == GameState.State.Ready)
        {
            postbox = FindObjectOfType<Postbox>();

            if (SaveHandler.LoadedFromSaveFile)
            {

            }
            else
            {
                BeginFromStart();
            }
        }
    }

    public void Aquired(string topic, ItemAmountPair payment)
    {
        Debug.Log(topic + " unlocked in the morning!");
        dailySacrificeExaused = true;
        InventoryManager.PlayerTryPay(payment.type, payment.amount);

        aquiredList.Add((topic, payment));

    }

    public void BeginFromStart()
    {
        lastLetterID = startingLetterID;
        letterProgressionState = LetterProgressionState.RecievedDay;
        if (postbox != null)
            SetPostboxLetterToID(lastLetterID);

        if (newOrderCrateSpawner != null)
            newOrderCrateSpawner.SpawnOrder(startingItems);
    }

    [Button]
    public void StartNextDay()
    {
        UpdateSacrifices();
        UpdateLetters();

        day++;
        SaveHandler.Save();
    }

    [Button]
    private void WifeRecievedLetter()
    {
        wifeRecievedLetter = true;
    }

    private void UpdateLetters()
    {
        if (postbox != null)
        {
            ItemAmountPair storedItem = postbox.GetStoredItem();
            var info = ItemsData.GetItemInfo(storedItem.type);
            postbox.SetStoredItem(ItemAmountPair.Nothing);

            //orders
            if (storedItem.type == ItemType.NewOrder)
            {
                if (ordersForNextDay.ContainsKey(storedItem.amount))
                {
                    List<ItemAmountPair> order = ordersForNextDay[storedItem.amount];
                    newOrderCrateSpawner.SpawnOrder(order);
                    ordersForNextDay.Remove(storedItem.amount);
                }
            }

            StepLetterProgression(storedItem.type == ItemType.LetterToFamily);

        }
        else
        {
            Debug.LogError("please reference the postbox in the progression handler");
        }
    }

    private void StepLetterProgression(bool sentLetterToWife)
    {
        //When out of content
        if (lastLetterID <= 0)
            return;

        //Update old state
        switch (letterProgressionState)
        {
            case LetterProgressionState.RecievedDay:
                if (sentLetterToWife)
                {
                    letterProgressionState = LetterProgressionState.WaitDay2;
                    wifeRecievedLetter = true;
                }
                else
                {
                    letterProgressionState = LetterProgressionState.WaitDay1;
                }

                break;
            case LetterProgressionState.WaitDay1:
                if (sentLetterToWife)
                    wifeRecievedLetter = true;

                letterProgressionState = LetterProgressionState.WaitDay2;
                break;
            case LetterProgressionState.WaitDay2:
                if (sentLetterToWife)
                    wifeRecievedLetter = true;
                letterProgressionState = LetterProgressionState.RecievedDay;
                break;
        }

        //Start new state
        switch (letterProgressionState)
        {
            case LetterProgressionState.RecievedDay:
                if (wifeRecievedLetter)
                    lastLetterID = LettersParser.GetLetterWithID(lastLetterID).AnswerId;
                else
                    lastLetterID = LettersParser.GetLetterWithID(lastLetterID).IgnoreId;

                if (lastLetterID > 0)
                    SetPostboxLetterToID(lastLetterID);
                wifeRecievedLetter = false;
                break;
        }
    }

    private void SetPostboxLetterToID(int id)
    {
        postbox.SetStoredItem(new ItemAmountPair(ItemType.LetterFromFamily, id));
    }

    [Button]
    private void DebugAquire()
    {
        Aquired(debugRewardToGet, ItemAmountPair.Nothing);
    }

    private void UpdateSacrifices()
    {
        dailySacrificeExaused = false;

        foreach (var aquired in aquiredList)
        {
            //Reward
            Debug.Log("Aquired: " + aquired.Item1 + " by paying with " + aquired.Item2.ToString());
            switch (aquired.Item1)
            {
                case "MiningSpeed":
                    digSpeedMultiplyer = rewardDigSpeedMultiplyer;
                    GameObject.FindObjectOfType<PickaxeAnimator>()?.Upgrade();
                    break;
                case "WalkingSpeed":
                    speedMultiplyer = rewardASpeedMultiplyer;
                    break;
                case "Strength":
                    strengthMultiplyer = rewardStrengthMultiplyer;
                    break;
                case "JumpHeight":
                    jumpMultiplyer = rewardJumpMultiplyer;
                    break;
                case "Spring":
                    isSpring = true;
                    overworldEffectHandler.MakeSpring();
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Snow, TileType.Grass);
                    break;
                case "MidasTouch":
                    isMidas = true;
                    //everything you touch turns to gold
                    break;

                case "Love":
                    hasLove = true;
                    GameObject.FindObjectOfType<Bed>()?.ChangeWakeUpText("I Love you."); // Move to some text/Dialog system
                    break;

                case "Victory":
                    hasWon = true;
                    //Open victory screen
                    break;

                case "AWayOut":
                    hasWayOut = true;
                    //Remove bedrock
                    break;

                case "Freedom":
                    isFree = true;
                    //save game finished somewhere, or corrupt files sth like that
                    Application.Quit();
                    break;

                default:
                    Debug.Log("Unimplemented aquired bonus: " + aquired);
                    break;
            }

            //consequence
            switch (aquired.Item2.type)
            {
                case ItemType.Support:
                    //Increase instability
                    break;

                case ItemType.LetterToFamily:
                    //Cannot send 
                    break;

                case ItemType.Family_Photo:
                    //No mail;
                    break;

                case ItemType.Hourglass:
                    //Your time?!
                    break;

                case ItemType.LetterFromFamily:
                    //analfabetism
                    break;

                case ItemType.Ball:
                    //Happyness
                    break;
                case ItemType.Globe:
                    //Everything
                    break;
            }

            sacrificeProgressionLevel++;
        }
        aquiredList.Clear();
    }

    public void RegisterOrder(int id, List<ItemAmountPair> itemAmountPairs)
    {
        ordersForNextDay.Add(id, itemAmountPairs);
    }

    public float GetPriceOf(string reward, string resource)
    {
        return 0; // SacrificePricesParser.GetPriceFor(reward, resource);
    }

    public SaveData ToSaveData()
    {
        ProgressionSaveData saveData = new ProgressionSaveData();
        saveData.GUID = GetSaveID();
        saveData.AquiredList = aquiredList;
        saveData.OrdersForNextDay = ordersForNextDay;
        saveData.SpeedMultiplyer = speedMultiplyer;
        saveData.DigSpeedMultiplyer = digSpeedMultiplyer;
        saveData.DailyPurchaseExaused = dailySacrificeExaused;
        saveData.Day = day;

        return saveData;
    }

    public void Load(SaveData data)
    {
        if (data is ProgressionSaveData saveData)
        {
            aquiredList = saveData.AquiredList;
            ordersForNextDay = saveData.OrdersForNextDay;
            speedMultiplyer = saveData.SpeedMultiplyer;
            digSpeedMultiplyer = saveData.DigSpeedMultiplyer;
            dailySacrificeExaused = saveData.DailyPurchaseExaused;
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

public enum LetterProgressionState
{
    RecievedDay,
    WaitDay1,
    WaitDay2
}


[System.Serializable]
public class ProgressionSaveData : SaveData
{
    public List<(string, ItemAmountPair)> AquiredList = new List<(string, ItemAmountPair)>();
    public Dictionary<int, List<ItemAmountPair>> OrdersForNextDay = new Dictionary<int, List<ItemAmountPair>>();

    public float SpeedMultiplyer = 1;
    public float DigSpeedMultiplyer = 1;
    public int Day;
    public bool DailyPurchaseExaused;
}