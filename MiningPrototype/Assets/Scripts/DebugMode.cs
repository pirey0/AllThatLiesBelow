using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Zenject;

public class DebugMode : MonoBehaviour
{
    public const bool DEBUG_POSSIBLE = true;
    static bool INIT = false;

    [SerializeField] List<GameObject> debugObjects;

    [Inject] PlayerStateMachine player;
    [Inject] PlayerInteractionHandler playerInteraction;
    [Inject] ProgressionHandler progressionHandler;

    bool open;
    private void Awake()
    {
        open = false;
        debugObjects.ForEach((x) => x.SetActive(false));
     
        if (!INIT)
        {
            DebugLogConsole.AddCommandInstance("/tp", "Teleport to " + Util.EnumToString(typeof(TeleportDestination)), "TeleportToAltar", this);
            DebugLogConsole.AddCommandInstance("/give", "Give player items " + Util.EnumToString(typeof(ItemType)), "PlayerGets", this);
            DebugLogConsole.AddCommandInstance("/kill", "Kill the player", "KillPlayer", this);
            DebugLogConsole.AddCommandInstance("/reward", "Get a reward without suffering the consequences " + Util.EnumToString(typeof(AltarRewardType)), "Reward", this);
            DebugLogConsole.AddCommandInstance("/sacrifice", "Sacrifice trade " + Util.EnumToString(typeof(AltarRewardType)) + " and " + Util.EnumToString(typeof(ItemType)), "Sacrifice", this);
            DebugLogConsole.AddCommandInstance("/sacrificeItem", "Sacrifice with no reward " + Util.EnumToString(typeof(ItemType)), "SacrificeItem", this);
            DebugLogConsole.AddCommandInstance("/sacrificeProgression", "Set the altar progression level. (Unlock different options 0-10)", "SetProgressionLevel", this);
            DebugLogConsole.AddCommandInstance("/deleteSave", "Delete your save file", "DeleteSaveFile", this);
            INIT = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (open)
            {
                open = false;
                debugObjects.ForEach((x) => x.SetActive(false));
            }
            else
            {
                open = true;
                debugObjects.ForEach((x) => x.SetActive(true));
            }
        }
    }

    private void OnGUI()
    {
        if (DEBUG_POSSIBLE && !open)
        {
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "F8: Debug Mode");
        }
    }

    private void TeleportToAltar(TeleportDestination destination)
    {
        Transform target = null;
        switch (destination)
        {
            case TeleportDestination.Altar:
                target = GameObject.FindObjectOfType<Altar>()?.transform;
                break;
            case TeleportDestination.Bed:
                target = FindObjectOfType<Bed>()?.transform;
                break;
        }

        if (target == null)
        {
            Debug.LogError("No " + destination + " found");
        }
        else
        {
            player.transform.position = target.position;
        }
    }

    public enum TeleportDestination
    {
        Bed,
        Altar
    }

    private void PlayerGets(ItemType itemType, int amount)
    {
        if (amount > 0)
            InventoryManager.PlayerCollects(itemType, amount);
    }
    
    private void KillPlayer()
    {
        player?.TakeDamage(DamageStrength.Strong);
    }


    private void SetProgressionLevel(int level)
    {
        progressionHandler.SetAltarProgressionLevel(level);
    }

    private void Reward(AltarRewardType reward)
    {
        progressionHandler.Aquired(reward.ToString(), ItemAmountPair.Nothing);
        progressionHandler.StartNextDay();
    }

    private void SacrificeItem( ItemType item)
    {
        progressionHandler.Aquired(AltarRewardType.None.ToString(), new ItemAmountPair(item, 9999));
        progressionHandler.StartNextDay();
    }

    private void Sacrifice(AltarRewardType reward, ItemType item)
    {
        progressionHandler.Aquired(reward.ToString(), new ItemAmountPair(item, 9999));
        progressionHandler.StartNextDay();
    }

    private void DeleteSaveFile()
    {
        SaveHandler.DestroySaveFile();
    }
}
