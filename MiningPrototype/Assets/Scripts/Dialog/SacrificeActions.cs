using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SacrificeActions : MonoBehaviour
{
    [SerializeField] float rewardASpeedMultiplyer, rewardDigSpeedMultiplyer, rewardStrengthMultiplyer, rewardJumpMultiplyer;
    [SerializeField] GameObject youWonPrefab;
    [SerializeField] TMPro.TMP_FontAsset fontAsset, fontAsset2;
    [SerializeField] PostProcessProfile noHappinessProfile;

    [Zenject.Inject] RuntimeProceduralMap map;
    [Zenject.Inject] OverworldEffectHandler effectHandler;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] CameraController cameraController;

    public void ApplyReward(AltarRewardType reward, ProgressionSaveData data)
    {
        switch (reward)
        {
            case AltarRewardType.MiningSpeed:
                data.digSpeedMultiplyer = rewardDigSpeedMultiplyer;
                GameObject.FindObjectOfType<PickaxeAnimator>(includeInactive: true).Upgrade();
                break;
            case AltarRewardType.WalkingSpeed:
                data.speedMultiplyer = rewardASpeedMultiplyer;
                break;
            case AltarRewardType.Strength:
                data.strengthMultiplyer = rewardStrengthMultiplyer;
                break;
            case AltarRewardType.JumpHeight:
                data.jumpMultiplyer = rewardJumpMultiplyer;
                break;
            case AltarRewardType.InstantDelivery:
                data.instantDelivery = true;
                break;
            case AltarRewardType.Spring:
                data.isSpring = true;
                effectHandler.MakeSpring();
                map.ReplaceAll(TileType.Snow, TileType.Grass);
                break;
            case AltarRewardType.MidasTouch:
                data.isMidas = true;
                //everything you touch turns to gold
                break;
            case AltarRewardType.Love:
                data.hasLove = true;
                GameObject.FindObjectOfType<Bed>()?.ChangeWakeUpText("I Love you."); // Move to some text/Dialog system
                break;
            case AltarRewardType.Victory:
                data.hasWon = true;
                Instantiate(youWonPrefab); //safe no injection needed
                break;
            case AltarRewardType.AWayOut:
                data.hasWayOut = true;
                map.ReplaceAll(TileType.BedStone, TileType.Stone);
                break;
            case AltarRewardType.Freedom:
                data.isFree = true;
                //save game finished somewhere, or corrupt files sth like that
                Application.Quit();
                break;
            default:
                Debug.Log("Unimplemented aquired bonus: " + reward);
                break;
        }
    }


    public void ApplyItemSacrificeConsequence(ItemType item, ProgressionSaveData data)
    {
        switch (item)
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
                inventoryManager.PlayerCollects(ItemType.Family_Photo_Empty, 1);
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
                map.ReplaceAll(TileType.Stone, TileType.SolidVoid);
                map.ReplaceAll(TileType.Grass, TileType.SolidVoid);
                map.ReplaceAll(TileType.Diamond, TileType.SolidVoid);
                map.ReplaceAll(TileType.Copper, TileType.SolidVoid);
                map.ReplaceAll(TileType.Gold, TileType.SolidVoid);
                cameraController.Camera.backgroundColor = Color.white;

                break;
        }
    }

}
