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
    [SerializeField] GameObject writeLetterOption;
    [SerializeField] NewOrderVisualizer newOrderVisualizerPrefab;
    [SerializeField] AudioSource letterWritingSource, paperFold;
    [SerializeField] SpriteAnimator animator;
    [SerializeField] SpriteAnimation idleAnimation, writeAnimation;
    [SerializeField] Transform optionsParent;
    [SerializeField] GameObject optionPrefab;

    [Zenject.Inject] Zenject.DiContainer diContainer;
    [Zenject.Inject] InventoryManager inventoryManager;
    [Zenject.Inject] ReadableItemHandler readableItemHandler;
    [Zenject.Inject] ProgressionHandler progressionHandler;

    List<GameObject> optionInstances;
    PlayerStateMachine seatedPlayer;
    NewOrderVisualizer currentOrder;
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
        RefreshOptions();
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

    private void RefreshOptions()
    {
        //Delete procedural children
        for (int i = optionsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsParent.GetChild(i).gameObject);
        }

        List<DeskOption> availableOptions = GetAvailableOptions();
        optionInstances = new List<GameObject>();

        foreach (var option in availableOptions)
        {
            optionInstances.Add(CreateButtonFor(option));
        }
    }


    private GameObject CreateButtonFor(DeskOption option)
    {
        var o = Instantiate(optionPrefab, optionsParent);
        o.name = option.Text;
        var text = o.GetComponentInChildren<TMPro.TMP_Text>();
        var button = o.GetComponentInChildren<UnityEngine.UI.Button>();
        text.text = option.Text;
        button.onClick.AddListener(option.Action);
        button.interactable = option.Active;

        return o;
    }

    private List<DeskOption> GetAvailableOptions()
    {
        var options = new List<DeskOption>();

        options.Add(new DeskOption("Leave Desk", LeaveDesk, true));
        options.Add(new DeskOption("Order New Equipment", FillOutOrder, true));

        if (!progressionHandler.GetVariable("Sent10Gold"))
        {
            options.Add(PaymentOption(10));
        }
        else if (!progressionHandler.GetVariable("Sent100Gold"))
        {
            options.Add(PaymentOption(100));
        }
        else if (!progressionHandler.GetVariable("Sent1000Gold"))
        {
            options.Add(PaymentOption(1000));
        }

        return options;
    }

    private DeskOption PaymentOption(int amount)
    {
        return new DeskOption("Send " + amount + " Gold to family", delegate { PayXGold(amount); }, inventoryManager.PlayerHas(ItemType.Gold, amount));
    }

    private void PayXGold(int amount)
    {
        if (inventoryManager.PlayerTryPay(ItemType.Gold, amount))
        {
            var type = amount == 10 ? LetterToFamily.LetterType.Payed10 : amount == 100 ? LetterToFamily.LetterType.Payed100 : LetterToFamily.LetterType.Payed1000;
            LetterToFamily order = new LetterToFamily(type);
            int id = readableItemHandler.AddNewReadable("Here are " + amount + " gold. \n With Love, \n Thomas");
            progressionHandler.RegisterSpecialLetter(id, order);
            inventoryManager.PlayerCollects(ItemType.LetterToFamily, id);

            LeaveDesk();

            Debug.Log("Created object for " + amount + " Gold to family");
        }
        else
        {
            Debug.Log("Sending money to family failed");
        }
    }

    private void SetPayedVariable(int amount)
    {
        progressionHandler.SetVariable("Sent" + amount + "Gold", true);
    }

    public void SitAtDesk(PlayerStateMachine playerToHide)
    {
        if (deskState == DeskState.Sitting)
            return;

        deskState = DeskState.Sitting;
        animator.Play(idleAnimation);
        optionsCanvas.gameObject.SetActive(true);

        foreach (var option in optionInstances)
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

    public void FillOutOrder()
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

        foreach (var option in optionInstances)
            option.SetActive(false);
    }

    [System.Obsolete]
    public void WriteLetterToFamily()
    {
        if (deskState == DeskState.WritingLetterForFamily)
            return;

        deskState = DeskState.WritingLetterForFamily;
        StartCoroutine("LetterWritingRoutine");
    }

    public IEnumerator LetterWritingRoutine()
    {
        foreach (var option in optionInstances)
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

        int readableId = readableItemHandler.AddNewOrder(order);
        progressionHandler.RegisterSpecialLetter(readableId, order);

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


public class DeskOption
{
    public string Text;
    public UnityEngine.Events.UnityAction Action;
    public bool Active;

    public DeskOption(string text, UnityEngine.Events.UnityAction action, bool active)
    {
        Text = text;
        Action = action;
        Active = active;
    }
}