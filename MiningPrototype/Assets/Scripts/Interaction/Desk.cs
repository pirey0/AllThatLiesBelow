using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite deskEmpty, deskFull;
    [SerializeField] Canvas optionsCanvas;
    [SerializeField] GameObject option1, option2;
    [SerializeField] NewOrderVisualizer newOrderVisualizerPrefab;
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
        spriteRenderer.sprite = deskFull;
        optionsCanvas.gameObject.SetActive(true);
        option1.SetActive(true);
        option2.SetActive(true);

        playerToHide.Disable();
    }

    public void LeaveDesk()
    {
        if (deskState == DeskState.Empty)
            return;

        deskState = DeskState.Empty;
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
            currentOrder = Instantiate(newOrderVisualizerPrefab, optionsCanvas.transform);
            currentOrder.Handshake(CloseNewOrder);
        }

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
        yield return new WaitForSeconds(3);

        string letterText = "My Dear Little Girl,\nI suppose by this time that you are sound asleep in [your] bunk and enjoying dreamland.Well, I’m not going to “waste” much time over this so after a few little scratches, will hit the sheets myself.\n" +
            "I do hope that when you receive this you will be feeling a little better than you were when I left. For the life of me, on the other hand, that perhaps something was wrong with the person leaving. \n" +
            "However, I’ll know tomorrow.\n" +
            "Now Little Girlie, I must take this out to the box and file into bed.It’s freezing tonight so I supposed that means snowshoeing.\n\n" +
            "Good night for this trip,\n" +
            "Reg";

        InventoryManager.PlayerCollects(ItemType.Family_Letter,ReadableItemHandler.AddNewReadable(letterText));
        LeaveDesk();
    }

    public void CloseNewOrder()
    {
        LeaveDesk();
    }
}
