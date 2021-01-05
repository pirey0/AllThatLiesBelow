using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public enum AltarRewardType
{
    None,
    InstantDelivery,
    Spring,
    MidasTouch,
    Analphabetism,
    Sadness
}

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
            case AltarRewardType.Analphabetism:
                fontAsset.material.SetFloat("_Sharpness", -1);
                fontAsset2.material.SetFloat("_Sharpness", -1);
                break;
            case AltarRewardType.Sadness:
                GameObject.FindObjectOfType<Bed>().SacrificedHappyness();
                cameraController.Camera.GetComponent<PostProcessVolume>().profile = noHappinessProfile;
                //Happyness
                break;
            default:
                Debug.Log("Unimplemented aquired bonus: " + reward);
                break;
        }
    }
}
