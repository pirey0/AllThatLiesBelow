using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;

public class ProgressionHandler : StateListenerBehaviour, ISavable, IDialogPropertiesHandler
{
    [ReadOnly] [SerializeField] string saveID = Util.GenerateNewSaveGUID();

    [SerializeField] NewOrderCrateSpawner newOrderCrateSpawner;
    [SerializeField] int startingLetterID = 100;
    [SerializeField] List<ItemAmountPair> startingItems;
    [SerializeField] PickaxeUpgrade[] pickaxeUpgrades;
    [SerializeField] AudioSource instantDeliveryAudio;
    [SerializeField] float timeMiningBeforePassageOfDay;

    [Zenject.Inject] OverworldEffectHandler overworldEffectHandler;
    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] LettersParser lettersParser;
    [Zenject.Inject] RuntimeProceduralMap map;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] SacrificeActions sacrificeActions;
    [Zenject.Inject] SceneAdder sceneAdder;
    [Zenject.Inject] PlayerStateMachine player;

    public System.Action<int> OnChangePickaxeLevel;

    ProgressionSaveData data;
    Letterbox letterBox;
    DropBox postbox;
    AltarDialogCollection altarDialogs;

    public int CurrentDay { get => data.day; }
    public int PickaxeLevel { get => data.pickaxeLevel; }
    public bool IsMidas { get => data.isMidas; }
    public float DigSpeedMultiplyer { get => data.digSpeedMultiplyer; }
    public AltarDialogCollection AltarDialogCollection { get => altarDialogs; }

    void Start()
    {
        altarDialogs = MiroParser.LoadTreesAsAltarTreeCollection();
    }

    protected override void OnPostSceneLoad()
    {
        letterBox = FindObjectOfType<Letterbox>();
        postbox = FindObjectOfType<DropBox>();
    }

    protected override void OnRealStart()
    {
        player.EnteredOverworld += OnEnterOverworld;
        player.LeftOverworld += OnLeftOverworld;
    }

    private void OnLeftOverworld()
    {
        Debug.Log("Player left overworld");
        data.leftOverworldTimestamp = Time.time;
    }

    private void OnEnterOverworld()
    {
        float t = (Time.time - data.leftOverworldTimestamp);
        Debug.Log("Player entered overworld after " + t + " seconds");
        if (t > timeMiningBeforePassageOfDay)
        {
            StartNextDay();
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

        letterBox = FindObjectOfType<Letterbox>();
        postbox = FindObjectOfType<DropBox>();

        OnChangePickaxeLevel?.Invoke(data.pickaxeLevel);
    }



    public void Aquired(string altarRewardType)
    {
        if (Enum.TryParse(altarRewardType, out AltarRewardType rewardType))
        {
            data.rewardsSacrificed.Add(rewardType);
            Debug.Log(rewardType + " added to rewards list");
        }
        else
        {
            Debug.LogError("Attempting to sacrifice unknown type: " + altarRewardType);
        }
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

    public bool GetVariable(string name)
    {
        if (name == "InEditor")
        {
            return Application.isEditor;
        }
        else if (name == "Debug")
        {
            return true;
        }

        if (data.variables.ContainsKey(name))
        {
            return data.variables[name];
        }
        else
        {
            return false;
        }
    }

    public void SetVariable(string name, bool value)
    {
        Debug.Log("Variable " + name + " set to " + value);
        if (data.variables.ContainsKey(name))
        {
            data.variables[name] = value;
        }
        else
        {
            data.variables.Add(name, value);
        }
    }

    public void FireEvent(string ev)
    {
        Debug.Log("AltarNode fired Event: " + ev);

        if (ev == "Camshake1")
        {
            cameraController.Shake(player.transform.position, CameraShakeType.explosion);
        }
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
                        Order order = data.ordersForNextDay[storedItem.amount];
                        if (order == null || (order.Items.Length == 0 && order.Upgrades.Length == 0))
                        {
                            Debug.LogError("There is an order ID with no elements: " + storedItem.amount);
                        }
                        else
                        {
                            newOrderCrateSpawner.SpawnOrder(new List<ItemAmountPair>(order.Items));
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
        foreach (var aquired in data.rewardsSacrificed)
        {
            Debug.Log("Aquired: " + aquired);
            data.rewardsReceived.Add(aquired);
            sacrificeActions.ApplyReward(aquired, data);
        }
        data.rewardsSacrificed.Clear();
    }


    public void RegisterOrder(int id, Order order)
    {
        if (data.instantDelivery)
        {
            instantDeliveryAudio.Play();
            newOrderCrateSpawner.SpawnOrder(new List<ItemAmountPair>(order.Items));
        }
        else
        {
            data.ordersForNextDay.Add(id, order);
        }
    }

    public float GetPriceOf(string reward, string resource)
    {
        return 0; // SacrificePricesParser.GetPriceFor(reward, resource);
    }

    public void Upgrade(ItemType type)
    {
        if (IsMaxUpgradeLevel(type))
            return;

        switch (type)
        {
            case ItemType.PickaxeUpgrade:
                data.pickaxeLevel++;
                data.digSpeedMultiplyer = GetMiningSpeedByPickaxeLevel(data.pickaxeLevel);
                OnChangePickaxeLevel?.Invoke(data.pickaxeLevel);
                break;
        }
    }

    public bool IsMaxUpgradeLevel(ItemType type)
    {
        if (type == ItemType.PickaxeUpgrade)
            return PickaxeLevel == 4;

        return false;
    }

    public int GetLevelForUpgrade(ItemType key)
    {
        switch (key)
        {
            case ItemType.PickaxeUpgrade:
                return PickaxeLevel;
        }

        return 0;
    }

    public float GetMiningSpeedByPickaxeLevel(int level)
    {
        foreach (var upgrade in pickaxeUpgrades)
        {
            if (upgrade.Level == level)
                return upgrade.MiningSpeed;
        }

        Debug.LogWarning("no mining speed info found for level " + level);
        return 100000;
    }

    public string GetDisplayNameForUpgrade(ItemType type)
    {
        foreach (var upgrade in pickaxeUpgrades)
        {
            if (upgrade.Type == type && upgrade.RequiredLevel == data.pickaxeLevel)
                return upgrade.DisplayName;
        }

        return "better " + type.ToString();
    }

    public SaveData ToSaveData()
    {
        data.GUID = GetSaveID();
        data.saveTimestamp = Time.time;
        return data;
    }

    public void Load(SaveData newData)
    {
        if (newData is ProgressionSaveData saveData)
        {
            this.data = saveData;
            data.leftOverworldTimestamp = Time.time - data.saveTimestamp + data.leftOverworldTimestamp;
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

    public void MarkRanDialog(string id)
    {
        data.dialogsRan.Add(id);
    }

    public bool HasRunDialog(string id)
    {
        return data.dialogsRan.Contains(id);
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

    //sacrifices
    public bool instantDelivery = false;
    public bool isSpring = false;
    public bool isMidas = false;
    public List<AltarRewardType> rewardsReceived = new List<AltarRewardType>();
    public List<AltarRewardType> rewardsSacrificed = new List<AltarRewardType>();
    public List<string> dialogsRan = new List<string>();

    //tutorial
    public List<string> achievedTutorials = new List<string>();

    //variables
    public Dictionary<string, bool> variables = new Dictionary<string, bool>();

    //letters and daily
    public Dictionary<int, Order> ordersForNextDay = new Dictionary<int, Order>();
    public int lastLetterID = -1;
    public bool wifeRecievedLetter = false;
    public LetterProgressionState letterProgressionState = LetterProgressionState.RecievedDay;
    public float leftOverworldTimestamp;
    public float saveTimestamp;

    //upgrades;
    public int pickaxeLevel = 1;
    public float digSpeedMultiplyer = 1;
}