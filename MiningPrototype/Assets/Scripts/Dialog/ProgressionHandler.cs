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
    [SerializeField] bool includeDebugDialogs;

    [SerializeField] int payed10LetterId;
    [SerializeField] int payed100LetterId;
    [SerializeField] int payed1000LetterId;

    [Zenject.Inject] EnvironmentEffectsHandler overworldEffectHandler;
    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] RuntimeProceduralMap map;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] SacrificeActions sacrificeActions;
    [Zenject.Inject] SceneAdder sceneAdder;
    [Zenject.Inject] PlayerStateMachine player;
    [Zenject.Inject] GameInstanceDataManger gameInstanceData;

    public event System.Action<int> OnChangePickaxeLevel;
    public event System.Action<int> OnChangeHelmetLevel;

    ProgressionSaveData data;
    Letterbox letterBox;
    DropBox postbox;
    AltarDialogCollection altarDialogs;

    public int CurrentDay { get => data.day; }
    public int PickaxeLevel { get => data.pickaxeLevel; }
    public int HelmetLevel { get => data.helmetLevel; }
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

        if (gameInstanceData.LoadBecauseOfDeath)
        {
            Debug.Log("Setting Died variable because load from death");
            SetVariable("Died", true);
        }
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

        int a = Mathf.FloorToInt(t / timeMiningBeforePassageOfDay);
        for (int i = 0; i < a; i++)
        {
            StartNextDayNoSacrificeUpdate();
        }
    }

    protected override void OnNewGame()
    {
        data = new ProgressionSaveData();

        var letter = LettersHolder.Instance.GetLetterWithID(startingLetterID);
        if (letter != null)
        {
            if (letterBox != null)
                ReceiveLetterWithID(startingLetterID);

            data.daysToNextLetter = letter.daysToNext;
            data.nextLetterID = letter.NextID;
        }

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



    public void AquireAltarReward(AltarRewardType altarRewardType)
    {
        data.rewardsSacrificed.Add(altarRewardType);
        Debug.Log(altarRewardType + " added to rewards list");
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
            return includeDebugDialogs;
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
        Debug.Log("Fired Event: " + ev);

        switch (ev)
        {
            case "Camshake1":
                cameraController.Shake(player.transform.position, CameraShakeType.explosion);
                break;

            case "Camshake2":
                cameraController.Shake(player.transform.position, CameraShakeType.hill, 5);
                break;

            case "Spring":
                AquireAltarReward(AltarRewardType.Spring);
                break;

            case "InstantDelivery":
                AquireAltarReward(AltarRewardType.InstantDelivery);
                break;

            case "ReceiveSpareHelmet":
                inventoryManager.PlayerCollects(ItemType.Helmet, 1);
                break;

            case "OreVision":
                StartCoroutine(OreVisibilityEffector.SetOreVisibleForCoroutine(map, 60));
                break;

            case "KillPlayer":
                player.TakeDamage(DamageStrength.Strong);
                break;



            default:
                Debug.LogWarning("Fired unimplemented event " + ev);
                break;
        }
    }


    [Button]
    public void StartNextDay()
    {
        UpdateSacrifices();
        StartNextDayNoSacrificeUpdate();
    }

    private void StartNextDayNoSacrificeUpdate()
    {
        UpdateLettersAndOrders();
        data.day++;
        Debug.Log("Starting day " + data.day);
    }


    private void UpdateLettersAndOrders()
    {
        if (postbox != null)
        {
            while (!postbox.IsEmpty())
            {
                ItemAmountPair storedItem = postbox.FetchItem();
                var info = ItemsData.GetItemInfo(storedItem.type);

                //orders
                if (storedItem.type == ItemType.NewOrder)
                {
                    if (data.specialLettersForNextDay.ContainsKey(storedItem.amount))
                    {
                        Order order = (Order)data.specialLettersForNextDay[storedItem.amount];
                        if (order == null || (order.Items.Length == 0 && order.Upgrades.Length == 0))
                        {
                            Debug.LogError("There is an order ID with no elements: " + storedItem.amount);
                        }
                        else
                        {
                            newOrderCrateSpawner.SpawnOrder(new List<ItemAmountPair>(order.Items));
                            CheckOrderForNarrativeTriggers(order);


                        }

                        data.specialLettersForNextDay.Remove(storedItem.amount);
                    }
                }
                else if (storedItem.type == ItemType.LetterToFamily)
                {
                    if (data.specialLettersForNextDay.ContainsKey(storedItem.amount))
                    {
                        LetterToFamily l = (LetterToFamily)data.specialLettersForNextDay[storedItem.amount];
                        ExcecuteLetterToFamilyConsequences(l);
                    }
                }
            }

            StepLetterProgression();
        }
        else
        {
            Debug.LogError("No Postbox found!");
        }
    }

    private void CheckOrderForNarrativeTriggers(Order order)
    {
        foreach (var item in order.Items)
        {
            switch (item.type)
            {
                case ItemType.Helmet:
                    SetVariable("BoughtHelmet", true);
                    break;
            }
        }

    }

    private void ExcecuteLetterToFamilyConsequences(LetterToFamily l)
    {
        if (l != null)
        {
            //Missing if happy or sad letter
            switch (l.Type)
            {
                case LetterToFamily.LetterType.Payed10:
                    if (!GetVariable("Sent10Gold"))
                    {
                        data.nextLetterID = payed10LetterId;
                        SetVariable("Sent10Gold", true);
                    }
                    break;

                case LetterToFamily.LetterType.Payed100:
                    if (!GetVariable("Sent100Gold"))
                    {
                        data.nextLetterID = payed100LetterId;
                        SetVariable("Sent100Gold", true);
                    }
                    break;

                case LetterToFamily.LetterType.Payed1000:
                    if (!GetVariable("Sent1000Gold"))
                    {
                        data.nextLetterID = payed1000LetterId;
                        SetVariable("Sent1000Gold", true);
                    }
                    break;
            }
        }
        else
        {
            Debug.LogError("Null Letter to family received.");
        }
    }

    private void StepLetterProgression()
    {
        if (data.unprocessedSentLetters.Count > 0)
        {
            Letter sent = LettersHolder.Instance.GetLetterWithID(data.unprocessedSentLetters[0]);
            if (sent != null)
            {
                Letter toRecv = LettersHolder.Instance.GetLetterWithID(sent.NextID);
                if (toRecv != null)
                {
                    ReceiveLetterWithID(sent.NextID);
                }
            }
            data.unprocessedSentLetters.RemoveAt(0);
        }
        else
        {
            if (data.daysToNextLetter > 0)
            {
                data.daysToNextLetter--;
            }
            else
            {
                Letter newLetter = LettersHolder.Instance.GetLetterWithID(data.nextLetterID);
                if (newLetter != null)
                {
                    ReceiveLetterWithID(newLetter.ID);
                    data.nextLetterID = newLetter.NextID;
                    data.daysToNextLetter = newLetter.daysToNext;
                }
            }
        }
    }

    private void ReceiveLetterWithID(int id)
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


    public void RegisterSpecialLetter(int id, ILetter letter)
    {
        if (data.instantDelivery && letter is Order order)
        {
            instantDeliveryAudio.Play();
            newOrderCrateSpawner.SpawnOrder(new List<ItemAmountPair>(order.Items));
        }
        else
        {
            data.specialLettersForNextDay.Add(id, letter);
        }
    }

    public void Upgrade(ItemType type)
    {
        switch (type)
        {
            case ItemType.IronPickaxe:
            case ItemType.SteelPickaxe:
            case ItemType.DiamondPickaxe:
                data.pickaxeLevel = GetLevelByUpgrade(type);
                data.digSpeedMultiplyer = GetMiningSpeedByPickaxeLevel(data.pickaxeLevel);
                OnChangePickaxeLevel?.Invoke(data.pickaxeLevel);
                break;

            case ItemType.Helmet:
                data.helmetLevel = 1;
                OnChangeHelmetLevel?.Invoke(data.helmetLevel);
                break;

            case ItemType.HeadLamp:
                data.helmetLevel = 2;
                OnChangeHelmetLevel?.Invoke(data.helmetLevel);
                break;
        }
    }

    public int GetLevelByUpgrade(ItemType key)
    {
        switch (key)
        {
            case ItemType.IronPickaxe:
                return 2;

            case ItemType.SteelPickaxe:
                return 3;
            case ItemType.DiamondPickaxe:
                return 4;
        }

        return 0;
    }

    public void UpgradePickaxe()
    {
        switch (PickaxeLevel)
        {
            case 1: Upgrade(ItemType.IronPickaxe); break;
            case 2: Upgrade(ItemType.SteelPickaxe); break;
            case 3: Upgrade(ItemType.DiamondPickaxe); break;
        }
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

    public void NotifyPlayerDeath()
    {
        gameInstanceData.LoadBecauseOfDeath = true;
    }

    public void Cheat_SetDigSpeedMultiplyer(float f)
    {
        data.digSpeedMultiplyer = f;
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
    public Dictionary<int, ILetter> specialLettersForNextDay = new Dictionary<int, ILetter>();
    public int nextLetterID = -1;
    public int daysToNextLetter = 0;
    public List<int> unprocessedSentLetters = new List<int>();

    public float leftOverworldTimestamp;
    public float saveTimestamp;

    //upgrades;
    public int pickaxeLevel = 1;
    public int helmetLevel = 0;
    public float digSpeedMultiplyer = 1;
}