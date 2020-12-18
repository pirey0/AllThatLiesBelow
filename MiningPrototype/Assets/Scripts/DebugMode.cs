using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using Zenject;

public class DebugMode : MonoBehaviour
{
    public const bool DEBUG_POSSIBLE = true;

    [SerializeField] List<GameObject> debugObjects;
    [SerializeField] GameObject debugChopper;

    [Inject] PlayerStateMachine player;
    [Inject] PlayerInteractionHandler playerInteraction;
    [Inject] ProgressionHandler progressionHandler;
    [Inject] InventoryManager inventoryManager;
    [Inject] TooltipHandler tooltipHandler;
    [Inject] CameraController cameraController;
    [Inject] RuntimeProceduralMap map;
    [Inject] PrefabFactory factory;

    Texture2D debugTex;
    bool open;
    bool showMap;
    bool showMapAdditiveLayer;

    private void Awake()
    {
        open = false;
        debugObjects.ForEach((x) => x.SetActive(false));
        debugTex = new Texture2D(map.SizeX, map.SizeY);
        debugTex.filterMode = FilterMode.Point;

        DebugLogConsole.AddCommandInstance("/tp", "Teleport to " + Util.EnumToString(typeof(TeleportDestination)), "TeleportToAltar", this);
        DebugLogConsole.AddCommandInstance("/give", "Give player items " + Util.EnumToString(typeof(ItemType)), "PlayerGets", this);
        DebugLogConsole.AddCommandInstance("/kill", "Kill the player", "KillPlayer", this);
        DebugLogConsole.AddCommandInstance("/reward", "Get a reward without suffering the consequences " + Util.EnumToString(typeof(AltarRewardType)), "Reward", this);
        DebugLogConsole.AddCommandInstance("/sacrifice", "Sacrifice trade " + Util.EnumToString(typeof(AltarRewardType)) + " and " + Util.EnumToString(typeof(ItemType)), "Sacrifice", this);
        DebugLogConsole.AddCommandInstance("/sacrificeItem", "Sacrifice with no reward " + Util.EnumToString(typeof(ItemType)), "SacrificeItem", this);
        DebugLogConsole.AddCommandInstance("/sacrificeProgression", "Set the altar progression level. (Unlock different options 0-10)", "SetProgressionLevel", this);
        DebugLogConsole.AddCommandInstance("/deleteSave", "Delete your save file", "DeleteSaveFile", this);
        DebugLogConsole.AddCommandInstance("/time", "sets time scale", "SetTimeScale", this);
        DebugLogConsole.AddCommandInstance("/showMap", "Visualizes the Map", "ShowMap", this);
        DebugLogConsole.AddCommandInstance("/showAdditiveMap", "Visualizes the Map", "ShowAdditiveMap", this);
        DebugLogConsole.AddCommandInstance("/chopper", "Spawn a Chopper", "SpawnChopper", this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
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
            tooltipHandler.Display(transform, p.ToString(), RuntimeProceduralMap.Instance[p].ToString());
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
            Vector2Int pos = player.transform.position.ToGridPosition();
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

    private void SpawnChopper()
    {
        var prevC = GameObject.FindObjectOfType<DebugChopper>();
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
        Altar,
        Mine
    }

    private void PlayerGets(ItemType itemType, int amount)
    {
        if (amount > 0)
            inventoryManager.PlayerCollects(itemType, amount);
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
        progressionHandler.Aquired(reward.ToString(), ItemAmountPair.Nothing, -1);
        progressionHandler.StartNextDay();
    }

    private void SacrificeItem(ItemType item)
    {
        progressionHandler.Aquired(AltarRewardType.None.ToString(), new ItemAmountPair(item, 9999), -1);
        progressionHandler.StartNextDay();
    }

    private void Sacrifice(AltarRewardType reward, ItemType item)
    {
        progressionHandler.Aquired(reward.ToString(), new ItemAmountPair(item, 9999), -1);
        progressionHandler.StartNextDay();
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
