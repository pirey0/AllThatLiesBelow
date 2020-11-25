using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class ProgressionHandler : StateListenerBehaviour, ISavable
{
    [ReadOnly] [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] float rewardASpeedMultiplyer, rewardDigSpeedMultiplyer, rewardStrengthMultiplyer, rewardJumpMultiplyer;
    [SerializeField] NewOrderCrateSpawner newOrderCrateSpawner;
    [SerializeField] int startingLetterID = 100;
    [SerializeField] List<ItemAmountPair> startingItems;
    [SerializeField] string debugRewardToGet;
    [SerializeField] ItemAmountPair debugCostForReward;
    [SerializeField] GameObject youWonPrefab;
    [SerializeField] TMPro.TMP_FontAsset fontAsset, fontAsset2;
    [SerializeField] PostProcessProfile noHappinessProfile;

    [Zenject.Inject] OverworldEffectHandler overworldEffectHandler;
    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] LettersParser lettersParser;

    ProgressionSaveData data;
    Postbox postbox;
    
    public float SpeedMultiplyer { get => data.speedMultiplyer; }
    public float DigSpeedMultiplyer { get => data.digSpeedMultiplyer; }

    public float JumpMultiplyer { get => data.jumpMultiplyer; }
    public bool DailySacrificeExpired { get => data.dailySacrificeExaused; }
    public int SacrificeProgressionLevel { get => data.sacrificeProgressionLevel; }
    public bool IsMidas { get => data.isMidas; }

    public bool InstableWorld { get => data.instableWorld; }

    public float ProgressionTimeScale { get => data.timeScale; }

    public List<string> RewardsReceived { get => data.rewardsReceived; }

    



    protected override void OnPostSceneLoad()
    {
        postbox = FindObjectOfType<Postbox>();
    }

    protected override void OnNewGame()
    {
        data = new ProgressionSaveData();
        data.lastLetterID = startingLetterID;
        data.letterProgressionState = LetterProgressionState.RecievedDay;
        if (postbox != null)
            SetPostboxLetterToID(data.lastLetterID);

        if (newOrderCrateSpawner != null)
            newOrderCrateSpawner.SpawnOrder(startingItems);
    }

    protected override void OnPostLoadFromFile()
    {
        //reapply all effects that need to be reapplied
    }

    public void Aquired(string topic, ItemAmountPair payment)
    {
        Debug.Log(topic + " unlocked in the morning!");
        data.dailySacrificeExaused = true;
        InventoryManager.PlayerTryPay(payment.type, payment.amount);

        data.aquiredList.Add((topic, payment));

    }
    public bool NeedsTutorialFor(string s)
    {
        return !data.achievedTutorials.Contains(s);
    }

    public void NotifyPassedTutorialFor(string s)
    {
        if (NeedsTutorialFor(s))
            data.achievedTutorials.Add(s);
    }

    [Button]
    public void StartNextDay()
    {
        UpdateSacrifices();
        UpdateLetters();

        data.day++;
        SaveHandler.Save();
    }

    [Button]
    private void WifeRecievedLetter()
    {
        data.wifeRecievedLetter = true;
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
                if (data.ordersForNextDay.ContainsKey(storedItem.amount))
                {
                    List<ItemAmountPair> order = data.ordersForNextDay[storedItem.amount];
                    newOrderCrateSpawner.SpawnOrder(order);
                    data.ordersForNextDay.Remove(storedItem.amount);
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
        if (data.lastLetterID <= 0)
            return;

        //Update old state
        switch (data.letterProgressionState)
        {
            case LetterProgressionState.RecievedDay:
                if (sentLetterToWife)
                {
                    data.letterProgressionState = LetterProgressionState.WaitDay2;
                    data.wifeRecievedLetter = true;
                }
                else
                {
                    data.letterProgressionState = LetterProgressionState.WaitDay1;
                }

                break;
            case LetterProgressionState.WaitDay1:
                if (sentLetterToWife)
                    data.wifeRecievedLetter = true;

                data.letterProgressionState = LetterProgressionState.WaitDay2;
                break;
            case LetterProgressionState.WaitDay2:
                if (sentLetterToWife)
                    data.wifeRecievedLetter = true;
                data.letterProgressionState = LetterProgressionState.RecievedDay;
                break;
        }

        //Start new state
        switch (data.letterProgressionState)
        {
            case LetterProgressionState.RecievedDay:
                if (data.wifeRecievedLetter)
                    data.lastLetterID = lettersParser.GetLetterWithID(data.lastLetterID).AnswerId;
                else
                    data.lastLetterID = lettersParser.GetLetterWithID(data.lastLetterID).IgnoreId;

                if (data.lastLetterID > 0)
                    SetPostboxLetterToID(data.lastLetterID);
                data.wifeRecievedLetter = false;
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
        Aquired(debugRewardToGet, debugCostForReward);
    }

    private void UpdateSacrifices()
    {
        data.dailySacrificeExaused = false;

        foreach (var aquired in data.aquiredList)
        {
            if (!data.rewardsReceived.Contains(aquired.Item1))
                data.rewardsReceived.Add(aquired.Item1);
            //Reward
            Debug.Log("Aquired: " + aquired.Item1 + " by paying with " + aquired.Item2.ToString());
            switch (aquired.Item1)
            {
                case "MiningSpeed":
                    data.digSpeedMultiplyer = rewardDigSpeedMultiplyer;
                    GameObject.FindObjectOfType<PickaxeAnimator>(includeInactive: true).Upgrade();
                    break;
                case "WalkingSpeed":
                    data.speedMultiplyer = rewardASpeedMultiplyer;
                    break;
                case "Strength":
                    data.strengthMultiplyer = rewardStrengthMultiplyer;
                    break;
                case "JumpHeight":
                    data.jumpMultiplyer = rewardJumpMultiplyer;
                    break;
                case "InstantDelivery":
                    data.instantDelivery = true;
                    break;
                case "Spring":
                    data.isSpring = true;
                    overworldEffectHandler.MakeSpring();
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Snow, TileType.Grass);
                    break;
                case "MidasTouch":
                    data.isMidas = true;
                    //everything you touch turns to gold
                    break;

                case "Love":
                    data.hasLove = true;
                    GameObject.FindObjectOfType<Bed>()?.ChangeWakeUpText("I Love you."); // Move to some text/Dialog system
                    break;

                case "Victory":
                    data.hasWon = true;
                    Instantiate(youWonPrefab);
                    //Open victory screen
                    break;

                case "AWayOut":
                    data.hasWayOut = true;
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.BedStone, TileType.Stone);
                    break;

                case "Freedom":
                    data.isFree = true;
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
                    data.instableWorld = true;
                    break;

                case ItemType.LetterToFamily:
                    //Cannot send
                    data.cannotSend = true;
                    GameObject.FindObjectOfType<Desk>(true).StopSending();
                    break;

                case ItemType.Family_Photo:
                    data.lastLetterID = -1;
                    InventoryManager.PlayerCollects(ItemType.Family_Photo_Empty, 1);
                    break;

                case ItemType.Hourglass:
                    Time.timeScale = 0.9f;
                    data.timeScale = 0.9f;
                    //Your time?!
                    break;

                case ItemType.LetterFromFamily:
                    //analfabetism
                    fontAsset.material.SetFloat("_Sharpness", -1);
                    fontAsset2.material.SetFloat("_Sharpness", -1);
                    break;

                case ItemType.Ball:
                    GameObject.FindObjectOfType<Bed>().SacrificedHappyness();
                    cameraController.Camera.GetComponent<PostProcessVolume>().profile = noHappinessProfile;
                    //Happyness
                    break;
                case ItemType.Globe:
                    //Everything
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Stone, TileType.SolidVoid);
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Grass, TileType.SolidVoid);
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Diamond, TileType.SolidVoid);
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Copper, TileType.SolidVoid);
                    RuntimeProceduralMap.Instance.ReplaceAll(TileType.Gold, TileType.SolidVoid);
                    cameraController.Camera.backgroundColor = Color.white;

                    break;
            }

            data.sacrificeProgressionLevel++;
        }
        data.aquiredList.Clear();
    }

    public void RegisterOrder(int id, List<ItemAmountPair> itemAmountPairs)
    {
        if (data.instantDelivery)
        {
            newOrderCrateSpawner.SpawnOrder(itemAmountPairs);
        }
        else
        {
            data.ordersForNextDay.Add(id, itemAmountPairs);
        }
    }

    public float GetPriceOf(string reward, string resource)
    {
        return 0; // SacrificePricesParser.GetPriceFor(reward, resource);
    }

    public SaveData ToSaveData()
    {
        ProgressionSaveData saveData = new ProgressionSaveData();
        saveData.GUID = GetSaveID();
        

        return saveData;
    }

    public void Load(SaveData newData)
    {
        if (newData is ProgressionSaveData saveData)
        {
            this.data = saveData;
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
    public int day = 0;
    
    //sacrifice
    public bool dailySacrificeExaused;
    public int sacrificeProgressionLevel = 1;
    public List<(string, ItemAmountPair)> aquiredList = new List<(string, ItemAmountPair)>();
    
    //sacrifice rewards
    public float speedMultiplyer = 1;
    public float digSpeedMultiplyer = 1;
    public float strengthMultiplyer = 1;
    public float jumpMultiplyer = 1;
    public bool instantDelivery = false;
    public bool isSpring = false;
    public bool isMidas = false;
    public bool hasLove = false;
    public bool hasWon = false;
    public bool hasWayOut = false;
    public bool isFree = false;
    public float timeScale = 1;
    public List<string> rewardsReceived = new List<string>();

    //sacriifce consequences
    public bool cannotSend;
    public bool paidEverything;
    public bool instableWorld;

    //tutorial
    public List<string> achievedTutorials = new List<string>();

    //letters and daily
    public Dictionary<int, List<ItemAmountPair>> ordersForNextDay = new Dictionary<int, List<ItemAmountPair>>();
    public int lastLetterID = -1;
    public bool wifeRecievedLetter = false;
    public LetterProgressionState letterProgressionState = LetterProgressionState.RecievedDay;
}