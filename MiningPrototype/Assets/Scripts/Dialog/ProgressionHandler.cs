using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;

public class ProgressionHandler : StateListenerBehaviour, ISavable
{
    [ReadOnly] [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] NewOrderCrateSpawner newOrderCrateSpawner;
    [SerializeField] int startingLetterID = 100;
    [SerializeField] List<ItemAmountPair> startingItems;
    [SerializeField] AudioSource instantDeliveryAudio;

    [Zenject.Inject] OverworldEffectHandler overworldEffectHandler;
    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] LettersParser lettersParser;
    [Zenject.Inject] RuntimeProceduralMap map;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] SacrificeActions sacrificeActions;
    [Zenject.Inject] SceneAdder sceneAdder;

    ProgressionSaveData data;
    Letterbox letterBox;
    DropBox postbox;
    List<Altar> altars;

    public int CurrentDay { get => data.day; }
    public float SpeedMultiplyer { get => data.speedMultiplyer; }
    public float DigSpeedMultiplyer { get => data.digSpeedMultiplyer; }
    public float StrengthMultiplyer { get => data.strengthMultiplyer; }

    public float JumpMultiplyer { get => data.jumpMultiplyer; }
    public bool DailySacrificeExpired { get => data.sacrificedAtID >= 0; }
    public int SacrificeProgressionLevel { get => data.sacrificeProgressionLevel; }
    public bool IsMidas { get => data.isMidas; }

    public bool InstableWorld { get => data.instableWorld; }

    public float ProgressionTimeScale { get => data.timeScale; }

    public List<string> RewardsReceived { get => data.rewardsReceived.Select((x) => x.ToString()).ToList(); }

    protected override void OnPostSceneLoad()
    {
        letterBox = FindObjectOfType<Letterbox>();
        postbox = FindObjectOfType<DropBox>();
    }

    protected override void OnRealStart()
    {
        var altarsArr = GameObject.FindObjectsOfType<Altar>();
        altars = new List<Altar>(altarsArr);
        altars.Sort((x, y) => (int)(x.transform.position.y - y.transform.position.y));

        for (int i = 0; i < altars.Count; i++)
        {
            altars[i].SetAltarID(i);
        }
    }

    protected override void OnNewGame()
    {
        data = new ProgressionSaveData();
        data.lastLetterID = startingLetterID;
        data.letterProgressionState = LetterProgressionState.RecievedDay;
        if (letterBox != null)
            SetPostboxLetterToID(data.lastLetterID);

        if (newOrderCrateSpawner != null)
            newOrderCrateSpawner.SpawnOrder(startingItems);
    }

    protected override void OnPostLoadFromFile()
    {
        //Reapply all effects on load
        foreach (var item in data.rewardsReceived)
        {
            sacrificeActions.ApplyReward(item, data);
        }

        foreach (var item in data.itemSacrificed)
        {
            sacrificeActions.ApplyItemSacrificeConsequence(item, data);
        }

        letterBox = FindObjectOfType<Letterbox>();
        postbox = FindObjectOfType<DropBox>();
    }

    public void NotifyAtarDiscovery(int id)
    {
        Debug.Log("Discovered Altar: " + id);
        if (data.lastFoundAltarID >= 0)
        {
            RemoveAltar(data.lastFoundAltarID);

            if (data.lastFoundAltarID < id)
                id -= 1; //as we remaped the list, the altar needs to go down by 1 if above the other
        }

        data.lastFoundAltarID = id;
    }

    public void Aquired(string topic, ItemAmountPair payment, int altarID)
    {
        Debug.Log(topic + " unlocked in the morning!");
        data.sacrificedAtID = altarID;
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
            bool sentToWife = false;
            while (!postbox.IsEmpty())
            {
                ItemAmountPair storedItem = postbox.FetchItem();
                var info = ItemsData.GetItemInfo(storedItem.type);

                //orders
                if (storedItem.type == ItemType.NewOrder)
                {
                    if (data.ordersForNextDay.ContainsKey(storedItem.amount))
                    {
                        List<ItemAmountPair> order = data.ordersForNextDay[storedItem.amount];
                        if (order == null || order.Count == 0)
                        {
                            Debug.LogError("There is an order ID with no elements: " + storedItem.amount);
                        }
                        else
                        {
                            newOrderCrateSpawner.SpawnOrder(order);
                        }

                        data.ordersForNextDay.Remove(storedItem.amount);
                    }
                }

                if (storedItem.type == ItemType.LetterToFamily)
                {
                    sentToWife = true;
                }
            }

            StepLetterProgression(sentToWife);
        }
        else
        {
            Debug.LogError("No Postbox found!");
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
        letterBox.AddStoredItem(new ItemAmountPair(ItemType.LetterFromFamily, id));
    }

    private void UpdateSacrifices()
    {

        foreach (var aquired in data.aquiredList)
        {
            Debug.Log("Aquired: " + aquired.Item1 + " by paying with " + aquired.Item2.ToString());

            if (Enum.TryParse(aquired.Item1, out AltarRewardType altarReward))
            {
                if (!data.rewardsReceived.Contains(altarReward))
                    data.rewardsReceived.Add(altarReward);

                sacrificeActions.ApplyReward(altarReward, data);
            }
            else
            {
                Debug.LogWarning("No altar reward named: " + aquired.Item1);
            }

            if (aquired.Item2.type != ItemType.None)
            {
                if (data.itemSacrificed.Contains(aquired.Item2.type))
                    data.itemSacrificed.Add(aquired.Item2.type);

                sacrificeActions.ApplyItemSacrificeConsequence(aquired.Item2.type, data);
            }

            data.sacrificeProgressionLevel++;
        }
        data.aquiredList.Clear();

        if (data.sacrificedAtID >= 0)
        {
            RemoveAltar(data.sacrificedAtID);
            data.sacrificedAtID = -1; //reset sacrifice id, allowing new sacrifices
            data.lastFoundAltarID = -1;
        }
    }

    private void RemoveAltar(int id)
    {
        var altar = altars[id];
        if (altar != null)
        {
            Debug.Log("Deleting old altar");
            Vector2Int pos = altar.transform.position.ToGridPosition();
            pos.x -= 5;
            pos.y -= 2;

            Destroy(altar);
            Util.IterateXY(10, (x, y) => map.SetMapAt(pos.x + x, pos.y + y, Tile.Make(TileType.Stone), TileUpdateReason.Generation));

            //remap altars list and altarID to properly handle loading and data.sacrificeAtID;
            altars.RemoveAt(id);
            for (int i = id; i < altars.Count; i++)
            {
                altars[i].SetAltarID(i);
            }
        }
    }

    /// <summary>
    /// Cheat to set the progression level.
    /// Used by Altar to decide viable Rewards.
    /// </summary>
    public void SetAltarProgressionLevel(int level)
    {
        data.sacrificeProgressionLevel = level;
    }

    public void RegisterOrder(int id, List<ItemAmountPair> itemAmountPairs)
    {
        if (data.instantDelivery)
        {
            instantDeliveryAudio.Play();
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
        data.GUID = GetSaveID();
        return data;
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

    public int GetLoadPriority()
    {
        return 20;
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
    public int sacrificedAtID = -1;
    public int lastFoundAltarID = -1;
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
    public List<AltarRewardType> rewardsReceived = new List<AltarRewardType>();
    public List<ItemType> itemSacrificed = new List<ItemType>();

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