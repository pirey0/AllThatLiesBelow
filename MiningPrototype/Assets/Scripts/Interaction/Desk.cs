using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite deskEmpty;
    [SerializeField] Canvas optionsCanvas;
    [SerializeField] GameObject option1, option2;
    [SerializeField] NewOrderVisualizer newOrderVisualizerPrefab;
    [SerializeField] AudioSource letterWritingSource, paperFold;
    [SerializeField] SpriteAnimator animator;
    [SerializeField] SpriteAnimation idleAnimation, writeAnimation;
    NewOrderVisualizer currentOrder;
    PlayerStateMachine seatedPlayer;

    DeskState deskState;

    private enum DeskState
    {
        Empty,
        Sitting,
        FillingOutOrder,
        WritingLetterForFamily,
    }

    public void BeginInteracting(GameObject interactor)
    {
        PlayerStateMachine player = interactor.GetComponent<PlayerStateMachine>();
        seatedPlayer = player;
        SitAtDesk(player);
    }

    public void EndInteracting(GameObject interactor)
    {
        PlayerStateMachine player = interactor.GetComponent<PlayerStateMachine>();
        LeaveDesk();
    }

    public void SubscribeToForceQuit(Action action)
    {
        //
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        //
    }

    public void SitAtDesk(PlayerStateMachine playerToHide)
    {
        if (deskState == DeskState.Sitting)
            return;

        deskState = DeskState.Sitting;
        animator.Play(idleAnimation);
        optionsCanvas.gameObject.SetActive(true);
        option1.SetActive(true);
        option2.SetActive(true);

        playerToHide.Disable();
        InventoryManager.ForcePlayerInventoryClose();
        seatedPlayer.transform.position = transform.position;
    }

    public void LeaveDesk()
    {
        if (deskState == DeskState.Empty)
            return;

        deskState = DeskState.Empty;
        animator.Play(null);
        spriteRenderer.sprite = deskEmpty;
        optionsCanvas.gameObject.SetActive(false);
        seatedPlayer.Enable();
    }

    public void FillOutOrder ()
    {
        if (deskState == DeskState.FillingOutOrder)
            return;

        deskState = DeskState.FillingOutOrder;

        if (currentOrder == null)
        {
            currentOrder = Instantiate(newOrderVisualizerPrefab);
            currentOrder.Handshake(CloseNewOrder);

            if (paperFold != null)
            {
                paperFold.pitch = 1;
                paperFold.Play();
            }
        }

        letterWritingSource.loop = true;
        letterWritingSource?.Play();
        animator.Play(writeAnimation);

        option1.SetActive(false);
        option2.SetActive(false);
    }

    public void WriteLetterToFamily()
    {
        if (deskState == DeskState.WritingLetterForFamily)
            return;

        deskState = DeskState.WritingLetterForFamily;
        StartCoroutine("LetterWritingRoutine");
    }

    public IEnumerator LetterWritingRoutine()
    {
        option1.SetActive(false);
        option2.SetActive(false);
        animator.Play(writeAnimation);
        letterWritingSource?.Play();
        yield return new WaitForSeconds(3);
        InventoryManager.PlayerCollects(ItemType.LetterToFamily, 1);
        LeaveDesk();
    }

    public void CloseNewOrder()
    {
        if (paperFold != null)
        {
            paperFold.pitch = 0.66f;
            paperFold.Play();
        }

        letterWritingSource.loop = false;
        letterWritingSource?.Stop();
        LeaveDesk();
    }
}
