using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite deskEmpty, deskFull;

    private enum DeskState
    {
        Empty,
        Sitting,
        FillingOutOrder,
        WritingLetterForFamily,
    }

    bool playerIsSittingAtDesk;

    public void BeginInteracting(GameObject interactor)
    {
        PlayerStateMachine player = interactor.GetComponent<PlayerStateMachine>();
        SitAtDesk(player);
    }

    public void EndInteracting(GameObject interactor)
    {
        PlayerStateMachine player = interactor.GetComponent<PlayerStateMachine>();
        LeaveDesk(player);
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
        if (playerIsSittingAtDesk)
            return;

        playerIsSittingAtDesk = true;
        spriteRenderer.sprite = deskFull;
        playerToHide.Disable();
    }

    public void LeaveDesk(PlayerStateMachine playerToEnableAgain)
    {
        if (!playerIsSittingAtDesk)
            return;

        playerIsSittingAtDesk = false;
        spriteRenderer.sprite = deskEmpty;
        playerToEnableAgain.Enable();
    }
}
