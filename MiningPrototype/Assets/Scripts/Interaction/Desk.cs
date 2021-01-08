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
    [SerializeField] GameObject[] options;
    [SerializeField] GameObject writeLetterOption;
    [SerializeField] NewOrderVisualizer newOrderVisualizerPrefab;
    [SerializeField] AudioSource letterWritingSource, paperFold;
    [SerializeField] SpriteAnimator animator;
    [SerializeField] SpriteAnimation idleAnimation, writeAnimation;
    NewOrderVisualizer currentOrder;
    PlayerStateMachine seatedPlayer;

    [Zenject.Inject] Zenject.DiContainer diContainer;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] ReadableItemHandler readableItemHandler;
    [Zenject.Inject] ProgressionHandler progressionHandler;

    DeskState deskState;
    private bool canSend = true;

    private event System.Action<IInteractable> InterruptInteraction;


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

    public void SubscribeToForceQuit(Action<IInteractable> action)
    {
        InterruptInteraction += action;
    }

    public void UnsubscribeToForceQuit(Action<IInteractable> action)
    {
        InterruptInteraction += action;
    }

    public void SitAtDesk(PlayerStateMachine playerToHide)
    {
        if (deskState == DeskState.Sitting)
            return;

        deskState = DeskState.Sitting;
        animator.Play(idleAnimation);
        optionsCanvas.gameObject.SetActive(true);

        foreach (var option in options)
            option.SetActive(true);

        if (writeLetterOption != null)
            writeLetterOption.SetActive(canSend);

        playerToHide.Disable();
        inventoryManager.ForcePlayerInventoryClose();
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
        InterruptInteraction?.Invoke(this);
    }

    public void FillOutOrder ()
    {
        if (deskState == DeskState.FillingOutOrder)
            return;

        deskState = DeskState.FillingOutOrder;
        animator.Play(writeAnimation);

        if (currentOrder == null)
        {
            currentOrder = diContainer.InstantiatePrefab(newOrderVisualizerPrefab).GetComponent<NewOrderVisualizer>();
            currentOrder.Handshake(FinishNewOrder, AbortNewOrder);

            if (paperFold != null)
            {
                paperFold.pitch = 1;
                paperFold.Play();
            }
        }

        foreach (var option in options)
            option.SetActive(false);
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
        foreach (var option in options)
            option.SetActive(false);

        animator.Play(writeAnimation);
        letterWritingSource?.Play();
        yield return new WaitForSeconds(3);
        inventoryManager.PlayerCollects(ItemType.LetterToFamily, 1);
        LeaveDesk();
    }

    public void FinishNewOrder(Order order)
    {
        if (paperFold != null)
        {
            paperFold.pitch = 0.66f;
            paperFold.Play();
        }

        if (letterWritingSource != null)
        {
            letterWritingSource.loop = true;
            letterWritingSource?.Play();
        }

        int readableId = readableItemHandler.AddNewReadable(order);
        progressionHandler.RegisterOrder(readableId, order);

        foreach (var singlePrice in order.Costs)
        {
            inventoryManager.PlayerTryPay(singlePrice.Key, singlePrice.Value);
        }

        StartCoroutine(FinishOrderRoutine(readableId));
    }

    public IEnumerator FinishOrderRoutine(int readableId)
    {
        yield return new WaitForSeconds(3);
        inventoryManager.PlayerCollects(ItemType.NewOrder, readableId);

        letterWritingSource.loop = false;
        letterWritingSource?.Stop();
        LeaveDesk();
    }

    public void AbortNewOrder()
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

    [Button]
    public void StopSending()
    {
        canSend = false;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
