using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class InventorySlotVisualizer : Button, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropReceiver
{
    [SerializeField] public Image icon;
    [SerializeField] public TMP_Text amountDisplay, shortcutDisplay;
    [SerializeField] ImageSpriteAnimator imageSpriteAnimator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameObject canDropOverlay, canNotDropOverlay;
    [SerializeField] AnimationCurve scaleOnOpenAndCloseCurve;

    [Inject] ProgressionHandler progressionHandler;
    [Inject] ItemPlacingHandler itemPlacingHandler;
    [Inject] ReadableItemHandler readableItemHandler;
    [Inject] TooltipHandler tooltipHandler;
    [Inject] CameraController cameraController;
    [Inject] InWorldCanvas inWorldCanvas;

    int amount;
    ItemType type;
    Inventory origin;
    RectTransform rectTransform;
    Transform parentRegular;
    Vector2 defaultAnchorPosition;

    Coroutine updateRoutine;
    Coroutine showTooltipRoutine;
    bool inDrag;
    bool showShortcut;
    private bool VisualsEnabled { get => icon.enabled; }

    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(ScaleCoroutine(scaleUp: true));
    }

    public void Display(ItemAmountPair pair, Inventory origin, bool showShortcut)
    {
        this.origin = origin;
        amount = pair.amount;
        type = pair.type;
        this.showShortcut = showShortcut;

        var info = ItemsData.GetItemInfo(type);

        if (icon != null)
        {
            if (info.AmountIsUniqueID && readableItemHandler.HasRead(amount))
            {
                SetIconSprite(info.DisplaySpriteRead);
            }
            else
            {
                SetIconSprite(info.DisplaySprite);
            }
        }

        if (amountDisplay != null && !info.AmountIsUniqueID)
        {
            amountDisplay.text = amount.ToString();
        }

        if (info.ShouldAnimate)
        {
            imageSpriteAnimator.Play(info.Animation);
            audioSource.clip = info.AudioClip;
            audioSource.Play();
        }

        if (info.Shortcut != KeyCode.None && showShortcut)
        {
            var shortCut = info.Shortcut.ToString();
            if (shortCut.StartsWith("Alpha"))
                shortCut = shortCut.Substring(5);
            shortcutDisplay.text = shortCut;
        }
    }

    private void SetIconSprite(Sprite sprite)
    {
        (icon.transform as RectTransform).sizeDelta = new Vector2((float)sprite.rect.width / 8f, (float)sprite.rect.height / 8f);
        icon.sprite = sprite;
    }

    public void Refresh()
    {
        Display(new ItemAmountPair(type, amount), origin, showShortcut);
    }

    public void SetButtonToSlot(UnityAction action)
    {
        onClick.AddListener(action);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        var info = ItemsData.GetItemInfo(type);
        if (info.AmountIsUniqueID && eventData.button == PointerEventData.InputButton.Right)
        {
            readableItemHandler.Display(amount, this);
        }
        else
        {
            DisplayTooltipText(info);
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (showTooltipRoutine == null)
            showTooltipRoutine = StartCoroutine(TooltipCounterRoutine());
    }

    IEnumerator TooltipCounterRoutine()
    {
        yield return new WaitForSeconds(0.66f);
        var info = ItemsData.GetItemInfo(type);
        DisplayTooltipText(info);
    }

    private void DisplayTooltipText(ItemInfo info)
    {
        string author = readableItemHandler.GetAuthor(amount);
        tooltipHandler?.Display(transform, info.DisplayName + ((author != null && info.AmountIsUniqueID) ? " <i>by " + author + "</i>" : ""), info.DisplayTooltip);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (showTooltipRoutine != null)
        {
            StopCoroutine(showTooltipRoutine);
            showTooltipRoutine = null;
        }

        tooltipHandler?.StopDisplaying(transform);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        tooltipHandler?.StopDisplaying(transform);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        defaultAnchorPosition = rectTransform.anchoredPosition;
        targetGraphic.raycastTarget = false;
        parentRegular = rectTransform.parent;
        rectTransform.SetParent(inWorldCanvas.transform, worldPositionStays: false);
        rectTransform.localScale = Vector3.one * 0.75f;
        inDrag = true;
        itemPlacingHandler.Show(new ItemAmountPair(type, amount), origin, OnPlacingHandlerCancel);
        StartCoroutine(UpdateInDrag());
    }

    private void OnPlacingHandlerCancel()
    {
        Debug.Log("Placing handler cancel");
        inDrag = false;
        ResetView();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inDrag)
        {
            rectTransform.position = Util.MouseToWorld(cameraController.Camera);
            UpdatePlacingPreview();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inDrag)
        {
            bool inUI = GetDistance() < 1;

            if (!inUI)
            {
                itemPlacingHandler.TryPlace(type, rectTransform.position);
            }
            itemPlacingHandler.Remove();
            inDrag = false;
            ResetView();
        }
    }

    private void ResetView()
    {
        EnableVisuals();
        rectTransform.anchoredPosition = defaultAnchorPosition;
        rectTransform.localScale = Vector3.one;
        canDropOverlay.SetActive(false);
        canNotDropOverlay.SetActive(false);
        rectTransform.SetParent(parentRegular, worldPositionStays: false);
        targetGraphic.raycastTarget = true;
    }

    //to also update when not moving the mouse
    private IEnumerator UpdateInDrag()
    {
        while (inDrag)
        {
            rectTransform.position = Util.MouseToWorld(cameraController.Camera);

            UpdatePlacingPreview();

            if (Input.GetMouseButtonDown(1)) //On right click while dragging <- dirty
            {
                if (amount > 0 && GetDistance() > 1)
                {
                    //drop half when pressing shift and 1 when not
                    int a = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? (amount / 2) : 1;
                    itemPlacingHandler.TryPlace(type, rectTransform.position, amount: a); //try place single item

                    //last torch placed
                    if (amount <= 1)
                        OnEndDrag(null); //dirty ++
                }
                else
                {
                    OnEndDrag(null); //dirty ++
                }
            }

            yield return null;
        }
    }

    private float GetDistance()
    {
        return (rectTransform.position - parentRegular.position).magnitude;
    }

    private void UpdatePlacingPreview()
    {
        float distance = GetDistance();
        var info = ItemsData.GetItemInfo(type);

        if (distance > 1)
        {
            if (itemPlacingHandler.IsAboveOtherReceiver())
            {
                EnableVisuals();
                bool canDrop = itemPlacingHandler.WouldBeReceived();
                canDropOverlay.SetActive(canDrop);
                canNotDropOverlay.SetActive(!canDrop);
                itemPlacingHandler.Hide();
            }
            else
            {
                if (VisualsEnabled)
                {
                    itemPlacingHandler.Show(new ItemAmountPair(type, amount), origin, OnPlacingHandlerCancel);
                    if (info.CanBePlaced)
                        DisableVisuals();

                    canDropOverlay.SetActive(false);
                    canNotDropOverlay.SetActive(false);
                }
            }

            itemPlacingHandler.UpdatePosition(rectTransform.position);

        }
        else
        {
            if (!VisualsEnabled)
            {
                itemPlacingHandler.Hide();
                EnableVisuals();
            }

            canDropOverlay.SetActive(false);
            canNotDropOverlay.SetActive(false);
        }
    }

    private void DisableVisuals()
    {
        image.color = Color.clear;
        icon.enabled = false;
        amountDisplay.enabled = false;

    }

    private void EnableVisuals()
    {
        image.color = Color.white;
        icon.enabled = true;
        amountDisplay.enabled = true;
    }

    public void CloseInventory()
    {
        if (this != null)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleCoroutine(scaleUp: false));
        }
    }

    IEnumerator ScaleCoroutine(bool scaleUp)
    {
        float timeMin = scaleOnOpenAndCloseCurve.keys[0].time;
        float timeMax = scaleOnOpenAndCloseCurve.keys[scaleOnOpenAndCloseCurve.length - 1].time;
        float time = (scaleUp ? timeMin : timeMax);

        while (scaleUp && time < timeMax || !scaleUp && time > timeMin)
        {
            time += (scaleUp ? 1 : -1) * Time.deltaTime;
            transform.localScale = Vector3.one * scaleOnOpenAndCloseCurve.Evaluate(time);
            yield return null;
        }


        if (!scaleUp)
            Destroy(gameObject);
    }

    public ItemAmountPair GetPair()
    {
        return new ItemAmountPair(type, amount);
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return origin.CanDeposit;
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        //
    }

    public void EndHover()
    {
        //
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair, Inventory origin)
    {
        if (origin.Contains(pair) && origin.TryRemove(pair))
            this.origin.Add(pair);
    }

    public bool IsSameInventory(Inventory inventory)
    {
        return origin == inventory;
    }


    public class Factory : Zenject.PlaceholderFactory<UnityEngine.Object, InventorySlotVisualizer>
    {
    }
}
