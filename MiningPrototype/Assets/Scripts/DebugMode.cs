using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Zenject;

public class DebugMode : MonoBehaviour
{
    public const bool DEBUG_POSSIBLE = true;
    [SerializeField] List<GameObject> debugObjects;

    [Inject] PlayerStateMachine player;
    [Inject] PlayerInteractionHandler playerInteraction;
    [Inject] ProgressionHandler progressionHandler;

    bool open;
    private void Awake()
    {
        open = false;
        debugObjects.ForEach((x) => x.SetActive(false));

        DebugLogConsole.AddCommandInstance("/tp", "Can teleport to:" + Util.EnumToString(typeof(TeleportDestination)), "TeleportToAltar", this);
        DebugLogConsole.AddCommandInstance("/give", "Give player items " + Util.EnumToString(typeof(ItemType)), "PlayerGets", this);
        DebugLogConsole.AddCommandInstance("/kill", "Kill the player", "KillPlayer", this);
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
}
