﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Zenject;

public class DebugMode : MonoBehaviour
{
    public const bool DEBUG_POSSIBLE = true;

    [SerializeField] List<GameObject> debugObjects;
    [SerializeField] GameObject debugChopper;

    [Inject] PlayerManager playerManager;
    [Inject] ProgressionHandler progressionHandler;
    [Inject] InventoryManager inventoryManager;
    [Inject] TooltipHandler tooltipHandler;
    [Inject] CameraController cameraController;
    [Inject] RuntimeProceduralMap map;
    [Inject] PrefabFactory factory;
    [Inject] StatsTracker statsTracker;
    [Inject] EnvironmentEffectsHandler environmentEffectsHandler;
    [Inject] CrumblingHandler crumblingHandler;

    Texture2D debugTex;
    bool open;
    bool showMap;
    bool showMapAdditiveLayer;
    List<string> commands = new List<string>();
    private void Awake()
    {
        open = false;
        debugObjects.ForEach((x) => x.SetActive(false));
        debugTex = new Texture2D(map.SizeX, map.SizeY);
        debugTex.filterMode = FilterMode.Point;

        AddCheat("/tp", "Teleport to " + Util.EnumToString(typeof(TeleportDestination)), "TeleportToAltar");
        AddCheat("/tpo", "Teleport to game object by name", "TeleportTo");
        AddCheat("/give", "Give player items " + Util.EnumToString(typeof(ItemType)), "PlayerGets");
        AddCheat("/kill", "Kill the player", "KillPlayer");
        AddCheat("/reward", "Get an altar reward" + Util.EnumToString(typeof(AltarRewardType)), "Reward");
        AddCheat("/deleteSave", "Delete your save file", "DeleteSaveFile");
        AddCheat("/time", "sets time scale", "SetTimeScale");
        AddCheat("/showMap", "Visualizes the Map", "ShowMap");
        AddCheat("/showAdditiveMap", "Visualizes the Map", "ShowAdditiveMap");
        AddCheat("/chopper", "Spawn a Chopper", "SpawnChopper");
        AddCheat("/stats", "Logs the stats", "LogStats");
        AddCheat("/upgradepickaxe", "Upgrades the pickaxe level","UpgradePickaxe");
        AddCheat("/setVariable", "Set a dialog variable", "SetDialogVariable");
        AddCheat("/efficiency", "Set the mining speed multiplyer", "SetMiningSpeed");
        AddCheat("/skipDay", "Skip a Day", "NextDay");
        AddCheat("/creative", "Enter Creative Mode", "CreativeMode");
        AddCheat("/survival", "Enter Survival Mode", "SurvivalMode");
        AddCheat("/toggleCrumble", "Switch Crumbling on and off", "ToggleCrumble");
    }

    private void ToggleCrumble()
    {
        crumblingHandler.TogglePaused();
    }

    private void CreativeMode()
    {
        playerManager.ChangePlayerTo(PlayerType.Creative);
    }

    private void SurvivalMode()
    {
        playerManager.ChangePlayerTo(PlayerType.Normal);
    }


    private void AddCheat(string command, string description, string methodname)
    {
        DebugLogConsole.AddCommandInstance(command, description, methodname, this);
        commands.Add(command);
    }

    private void OnDestroy()
    {
        foreach (var item in commands)
        {
            DebugLogConsole.RemoveCommand(item);
        }
        commands.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8) || Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (open)
            {
                open = false;
                debugObjects.ForEach((x) => x.SetActive(false));
                tooltipHandler.StopDisplaying(transform);
            }
            else
            {
                open = true;
                debugObjects.ForEach((x) => x.SetActive(true));
            }
        }

        if (open)
        {
            var p = Util.MouseToWorld(cameraController.Camera).ToGridPosition();
            tooltipHandler.Display(transform, p.ToString(), map[p].ToString());
        }
    }

    private void OnGUI()
    {
        if (DEBUG_POSSIBLE && !open)
        {
            GUI.Label(new Rect(Screen.width - 100, 10, 100, 30), "F8: Debug Mode");
        }
        else if (open && showMap)
        {
            //Updating texture OnGUI
            Vector2Int pos = playerManager.GetPlayer().transform.position.ToGridPosition();
            for (int y = 0; y < debugTex.height; y++)
            {
                for (int x = 0; x < debugTex.width; x++)
                {
                    if (x == pos.x && y == pos.y)
                    {
                        debugTex.SetPixel(x, y, Color.red);
                    }
                    else
                    {
                        Color c = Color.black;
                        if (showMapAdditiveLayer)
                            c = map.IsAdditivelyCoveredAt(x, y) ? Color.red : Color.black;
                        else
                            c = MapHelper.TileToColor(map[x, y].Type);
                        debugTex.SetPixel(x, y, c);
                    }
                }
            }

            debugTex.Apply();
            GUI.DrawTexture(new Rect(Screen.width - debugTex.width * 3, 0, debugTex.width * 3, debugTex.height * 3), debugTex);
        }
    }

    private void SetMiningSpeed(float mult)
    {
        progressionHandler.Cheat_SetDigSpeedMultiplyer(mult);
    }
    private void LogStats()
    {
        statsTracker.LogToConsole();
    }

    private void SpawnChopper()
    {
        var prevC = GameObject.FindObjectOfType<DebugChopper>();
        var player = playerManager.GetPlayer();
        if (prevC != null)
        {
            player.ExitVehicle(prevC);
            Destroy(prevC.gameObject);
        }
        else
        {
            var c = factory.Create(debugChopper, player.transform.position + new Vector3(0, 4, 0), Quaternion.identity);
            player.EnterVehicle(c.GetComponent<IVehicle>());
        }
    }

    private void NextDay()
    {
        progressionHandler.StartNextDay();
    }
    private void ShowMap()
    {
        showMap = !showMap;
        showMapAdditiveLayer = false;
    }

    private void ShowAdditiveMap()
    {
        showMap = !showMap;
        showMapAdditiveLayer = true;
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
            case TeleportDestination.Mine:
                target = FindObjectOfType<Torch>()?.transform;
                break;
            case TeleportDestination.InFrontOfMine:
                target = LocationIndicator.Find(IndicatorType.InFrontOfMine)?.transform;
                break;
        }

        if (target == null)
        {
            Debug.LogError("No " + destination + " found");
        }
        else
        {
            playerManager.TeleportPlayerTo(target.position);
            environmentEffectsHandler.UpdateOverworldEffects();
            environmentEffectsHandler.UpdateJungleEffects();
        }
    }

    public void TeleportTo(string name)
    {
        GameObject target = GameObject.Find(name);
        if (target != null)
        {
            playerManager.TeleportPlayerTo(target.transform.position);
            environmentEffectsHandler.UpdateOverworldEffects();
            environmentEffectsHandler.UpdateJungleEffects();
        }
    }

    public enum TeleportDestination
    {
        Bed,
        Altar,
        Mine,
        InFrontOfMine
    }

    private void PlayerGets(ItemType itemType, int amount)
    {
        if (amount > 0)
            inventoryManager.PlayerCollects(itemType, amount);
    }

    private void KillPlayer()
    {
        playerManager.GetPlayer()?.TakeDamage(DamageStrength.Strong);
    }

    private void UpgradePickaxe()
    {
        progressionHandler.UpgradePickaxe();
    }

    private void Reward(AltarRewardType reward)
    {
        progressionHandler.AquireAltarReward(reward);
        progressionHandler.StartNextDay();
    }

    private void SetDialogVariable(string name , bool value)
    {
        progressionHandler.SetVariable(name, value);
    }

    private void DeleteSaveFile()
    {
        SaveHandler.DestroySaveFile();
    }

    private void SetTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;
    }
}
