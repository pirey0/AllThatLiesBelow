using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bed : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Hut hut;
    [SerializeField] Image nightFadeToBlack;

    public void BeginInteracting(GameObject interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();

        if (hut.IsOpen())
            EnterBed(player);
    }

    public void EndInteracting(GameObject interactor)
    {
        PlayerController player = interactor.GetComponent<PlayerController>();

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

    private void EnterBed(PlayerController playerToHide)
    {
        playerToHide.Hide();
        spriteRenderer.enabled = true;
        StartCoroutine(SleepCoroutine(playerToHide));
    }

    private void LeaveBed(PlayerController playerToEnableAgain)
    {
        playerToEnableAgain.Show();
        spriteRenderer.enabled = false;
    }

    IEnumerator SleepCoroutine(PlayerController playerToEnableAgain)
    {
        if (nightFadeToBlack!= null)
        {
            float nightOpacity = 0f;

            while (nightOpacity < 1f)
            {
                nightOpacity += Time.deltaTime;
                nightFadeToBlack.color = new Color(0,0,0,nightOpacity);

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            while (nightOpacity > 0f)
            {
                nightOpacity -= Time.deltaTime;
                nightFadeToBlack.color = new Color(0, 0, 0, nightOpacity);

                yield return null;
            }

        }

        LeaveBed(playerToEnableAgain);
    }
}
