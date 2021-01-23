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
    [Zenject.Inject] EnvironmentEffectsHandler effectHandler;
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
                effectHandler.MakeSpring();
                data.isSpring = true;
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
                //Happyness
                break;
            default:
                Debug.Log("Unimplemented aquired bonus: " + reward);
                break;
        }
    }

    public void ApplyRewardFromLoading(AltarRewardType reward, ProgressionSaveData data)
    {
        switch (reward)
        {
            case AltarRewardType.InstantDelivery:
                break;
            case AltarRewardType.Spring:
                effectHandler.MakeSpring();
                break;
            case AltarRewardType.MidasTouch:
                break;
            case AltarRewardType.Analphabetism:
                fontAsset.material.SetFloat("_Sharpness", -1);
                fontAsset2.material.SetFloat("_Sharpness", -1);
                break;
                //Happyness
                break;
            default:
                Debug.Log("Unimplemented aquired bonus: " + reward);
                break;
        }
    }
}
