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

    [Inject] ProgressionHandler progressionHandler;
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

    public void SubscribeToForceQuit(Action action)
    {
        //action += LeaveBed;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        //action -= LeaveBed;
    }

    private void EnterBed(PlayerStateMachine playerToHide)
    {
        playerToHide.Disable();
        InventoryManager.ForcePlayerInventoryClose();
        spriteRenderer.sprite = sleeping;
        StartCoroutine(SleepCoroutine(playerToHide));
    }

    private void LeaveBed(PlayerStateMachine playerToEnableAgain)
    {
        playerToEnableAgain.Enable();
        spriteRenderer.sprite = empty;
    }

    IEnumerator SleepCoroutine(PlayerStateMachine playerToEnableAgain)
    {
        playerToEnableAgain.transform.position = transform.position;

        if (nightFadeToBlack != null)
        {
            float nightOpacity = 0f;


            while (nightOpacity < 1f)
            {
                nightOpacity += Time.deltaTime;
                nightFadeToBlack.color = new Color(0, 0, 0, nightOpacity);

                yield return null;
            }

            wakeupSound?.Play();

            yield return new WaitForSeconds(0.25f);
            progressionHandler.StartNextDay();
            spriteRenderer.sprite = wakeup;
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
}
