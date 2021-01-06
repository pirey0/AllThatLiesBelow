using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class AltarVisualizer : MonoBehaviour
{
    DialogVisualizer dialogVisualizer;

    [SerializeField] AltarSkin skin;

    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] AudioSource audioSource;

    [SerializeField] AltarSkinInfo[] skins;

    [Zenject.Inject] PlayerInteractionHandler playerInteractionHandler;

    private void OnEnable()
    {
        dialogVisualizer = FindObjectOfType<DialogVisualizer>();

        if (dialogVisualizer == null)
        {
            Debug.LogError("No dialog Visualizer found for altar.");
            Destroy(gameObject);
        }
        else
            dialogVisualizer.OnChangeState += OnChangeState;
    }

    private void OnDisable()
    {
        if (dialogVisualizer == null)
            Destroy(gameObject);
        else
            dialogVisualizer.OnChangeState -= OnChangeState;
    }

    private void OnChangeState (AltarState altarState)
    {
        AltarVisualStateInfo info = GetInfoForState(altarState);
        if (info == null)
            Debug.LogWarning("no skin and state info defined for " + skin + " and " + altarState);
        else
        {
            if (spriteAnimator.Animation != info.Animation)
                spriteAnimator.Play(info.Animation);

            if (audioSource.clip != info.AudioClip)
                audioSource.clip = info.AudioClip; if (audioSource.clip != null) { audioSource.Play(); }

            spriteAnimator.Renderer.flipX = info.lookAtPlayer ? playerInteractionHandler.transform.position.x < transform.position.x : false;

        }
    }

    private AltarVisualStateInfo GetInfoForState(AltarState altarState)
    {
        foreach (var skinInfo in skins)
        {
            if (skin == skinInfo.Skin)
            {
                foreach (var stateInfo in skinInfo.States)
                {
                    if (stateInfo.State == altarState)
                        return stateInfo;
                }
            }
        }

        return null;
    }
}

public enum AltarState
{
    Passive,
    Talking,
    Idle
}

public enum AltarSkin
{
    Miner,
}

[System.Serializable]
public class AltarSkinInfo
{
    public AltarSkin Skin;
    public AltarVisualStateInfo[] States;
}

[System.Serializable]
public class AltarVisualStateInfo
{
    public AltarState State;
    public SpriteAnimation Animation;
    public AudioClip AudioClip;
    public bool lookAtPlayer;
}
