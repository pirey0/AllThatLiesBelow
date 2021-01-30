using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Bed : StateListenerBehaviour, IInteractable
{
    [SerializeField] AudioSource wakeupSound;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite empty, sleeping, wakeup, badDream;
    [SerializeField] Hut hut;
    [SerializeField] Image nightFadeToBlack;
    [SerializeField] float sleepCooldown = 60;

    [SerializeField] string wakeUpText;

    [Zenject.Inject] TransitionEffectHandler transitionEffectHandler;
    [Inject] ProgressionHandler progressionHandler;
    [Inject] InventoryManager inventoryManager;
    [Inject] SaveHandler saveHandler;
    [Inject] EnvironmentEffectsHandler effectHandler;
    [Inject] PlayerStatementsHandler playerStatements;
    [Inject] CursorHandler cursorHandler;
    [Inject] GameInstanceDataManger gameInstanceData;
    [Inject] PlayerManager playerManager;

    string defaultWakeUpTest;
    float lastSleepTimeStamp = -10000;

    private event System.Action<IInteractable> ForceInterrupt;

    private void Start()
    {
        defaultWakeUpTest = wakeUpText;
    }

    protected override void OnRealStart()
    {
        if (gameInstanceData.LoadBecauseOfDeath)
        {
            transitionEffectHandler.FadeIn(FadeType.Nightmare);
            WakeUpFromNightmare(playerManager.GetPlayer());
        }
    }

    public void BeginInteracting(IPlayerController player)
    {
        if (hut.IsOpen() && Time.time - lastSleepTimeStamp > sleepCooldown)
        {
            lastSleepTimeStamp = Time.time;
            EnterBed(player);
        }
        else
        {
            LeaveBed(player);
            playerStatements.Say("I just woke up...", 3f);
        }
    }

    public void EndInteracting(IPlayerController player)
    {
        LeaveBed(player);
    }

    public void SubscribeToForceQuit(Action<IInteractable> action)
    {
        ForceInterrupt += action;
    }

    public void UnsubscribeToForceQuit(Action<IInteractable> action)
    {
        ForceInterrupt -= action;
    }

    private void EnterBed(IPlayerController playerToHide)
    {
        playerToHide.Disable();
        cursorHandler.Hide();

        inventoryManager.ForcePlayerInventoryClose();
        spriteRenderer.sprite = sleeping;
        StartCoroutine(SleepCoroutine(playerToHide));
    }

    private void LeaveBed(IPlayerController playerToEnableAgain)
    {
        ForceInterrupt?.Invoke(this);
        playerToEnableAgain.Enable();
        cursorHandler.Show();
        spriteRenderer.sprite = empty;
    }

    IEnumerator SleepCoroutine(IPlayerController playerToEnableAgain)
    {
        playerToEnableAgain.transform.position = transform.position;

        if (nightFadeToBlack != null)
        {
            nightFadeToBlack.enabled = true;

            float nightOpacity = 0f;


            while (nightOpacity < 1f)
            {
                nightOpacity += Time.deltaTime;
                nightFadeToBlack.color = new Color(0, 0, 0, nightOpacity);

                yield return null;
            }

            progressionHandler.StartNextDay();
            effectHandler.SetDay();
            playerToEnableAgain.CanDig = true;
            playerToEnableAgain.Enable();
            yield return null;
            saveHandler.Save();
            playerToEnableAgain.Disable();

            wakeupSound?.Play();

            yield return new WaitForSeconds(0.25f);

            spriteRenderer.sprite = wakeup;

            playerStatements.Say(wakeUpText, 7f);

            yield return new WaitForSeconds(1f);

            while (nightOpacity > 0f)
            {
                nightOpacity -= Time.deltaTime * 0.33f;
                nightFadeToBlack.color = new Color(0, 0, 0, nightOpacity);

                yield return null;
            }
        }
        LeaveBed(playerToEnableAgain);
    }

    internal void WakeUpFromNightmare(IPlayerController player)
    {
        player.Disable();
        StartCoroutine(NightmareCoroutine(player));
    }

    IEnumerator NightmareCoroutine(IPlayerController playerToEnableAgain)
    {
        spriteRenderer.sprite = badDream;
        yield return new WaitForSeconds(5f);
        LeaveBed(playerToEnableAgain);
    }

    public void ChangeWakeUpText(string newWakeUpText)
    {
        wakeUpText = newWakeUpText;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

}
