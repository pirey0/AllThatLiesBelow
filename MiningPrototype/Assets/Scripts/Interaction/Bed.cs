using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Bed : MonoBehaviour, IInteractable
{
    [SerializeField] AudioSource wakeupSound;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite empty, sleeping, wakeup, badDream;
    [SerializeField] Hut hut;
    [SerializeField] Image nightFadeToBlack;

    [SerializeField] string wakeUpText;

    [Zenject.Inject] TransitionEffectHandler transitionEffectHandler;
    [Inject] ProgressionHandler progressionHandler;
    [Inject] InventoryManager inventoryManager;
    [Inject] SaveHandler saveHandler;
    [Inject] EnvironmentEffectsHandler effectHandler;
    [Inject] PlayerStatementsHandler playerStatements;
    [Inject] CursorHandler cursorHandler;

    string defaultWakeUpTest;
    bool sacrificedHappyness = false;

    private event System.Action<IInteractable> ForceInterrupt;

    private void Start()
    {
        defaultWakeUpTest = wakeUpText;
    }
    public void BeginInteracting(GameObject interactor)
    {
        PlayerStateMachine player = interactor.GetComponent<PlayerStateMachine>();

        if (hut.IsOpen())
            EnterBed(player);
    }

    public void EndInteracting(GameObject interactor)
    {
        PlayerStateMachine player = interactor.GetComponent<PlayerStateMachine>();

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

    private void EnterBed(PlayerStateMachine playerToHide)
    {
        playerToHide.Disable();
        cursorHandler.Hide();

        inventoryManager.ForcePlayerInventoryClose();
        spriteRenderer.sprite = sleeping;
        StartCoroutine(SleepCoroutine(playerToHide));
    }

    private void LeaveBed(PlayerStateMachine playerToEnableAgain)
    {
        ForceInterrupt?.Invoke(this);
        playerToEnableAgain.Enable();
        cursorHandler.Show();
        spriteRenderer.sprite = empty;
    }

    IEnumerator SleepCoroutine(PlayerStateMachine playerToEnableAgain)
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

            if (sacrificedHappyness) {
                transitionEffectHandler.FadeIn(FadeType.Nightmare);
                nightFadeToBlack.color = new Color(0, 0, 0, 0);
            }
            else
                wakeupSound?.Play();

            yield return new WaitForSeconds(0.25f);

            spriteRenderer.sprite = sacrificedHappyness? badDream:wakeup;

            playerStatements.Say(wakeUpText,7f);

            if (!sacrificedHappyness)
            {
                yield return new WaitForSeconds(1f);

                while (nightOpacity > 0f)
                {
                    nightOpacity -= Time.deltaTime * 0.33f;
                    nightFadeToBlack.color = new Color(0, 0, 0, nightOpacity);

                    yield return null;
                }

            } else
            {
                yield return new WaitForSeconds(5f);
            }

        }
        LeaveBed(playerToEnableAgain);
    }

    internal void WakeUpFromNightmare(GameObject gameObject)
    {
        PlayerStateMachine playerToHide = gameObject.GetComponent<PlayerStateMachine>();
        playerToHide.Disable();
        StartCoroutine(NightmareCoroutine(playerToHide));
    }

    IEnumerator NightmareCoroutine(PlayerStateMachine playerToEnableAgain)
    {
       spriteRenderer.sprite = badDream;
       yield return new WaitForSeconds(5f);
       LeaveBed(playerToEnableAgain);
    }

    public void ChangeWakeUpText(string newWakeUpText)
    {
        wakeUpText = newWakeUpText;
    }

    [Button]
    public void SacrificedHappyness()
    {
        if (wakeUpText == defaultWakeUpTest)
            wakeUpText = "And yet another day of pain.";

        sacrificedHappyness = true;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
