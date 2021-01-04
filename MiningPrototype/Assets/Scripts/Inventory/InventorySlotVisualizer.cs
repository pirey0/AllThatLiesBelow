using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System.Text.RegularExpressions;

public class InventorySlotVisualizer : Button, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropReceiver
{
    [SerializeField] public Image icon;
    [SerializeField] public TMP_Text amountDisplay;
    [SerializeField] GameObject canDropOverlay, canNotDropOverlay;
    [SerializeField] AnimationCurve scaleOnOpenAndCloseCurve;


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

    public void Display(ItemAmountPair pair, Inventory origin)
    {
        this.origin = origin;
        amount = pair.amount;
        type = pair.type;

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
    }

    private void SetIconSprite(Sprite sprite)
    {
        (icon.transform as RectTransform).sizeDelta = new Vector2((float)sprite.rect.width / 8f, (float)sprite.rect.height / 8f);
        icon.sprite = sprite;
    }

    public void Refresh()
    {
        Display(new ItemAmountPair(type, amount), origin);
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
            readableItemHandler.Display(amount, this);
        else
            tooltipHandler?.Display(transform, info.DisplayName, info.DisplayTooltip);
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
        tooltipHandler?.Display(transform, info.DisplayName, info.DisplayTooltip);
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
        rectTransform.SetParent(inWorldCanvas.transform, worldPositionStays: false); //hope your eyes are okay now.
        rectTransform.localScale = Vector3.one * 0.75f;
        inDrag = true;
        StartCoroutine(UpdateInDrag());
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Util.MouseToWorld(cameraController.Camera);
        UpdatePlacingPreview();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool inUI = GetDistance() < 2;

        if (!inUI)
        {
            itemPlacingHandler.TryPlace(type, rectTransform.position);
        }
        itemPlacingHandler.Hide(resetHeldItem: true);
        inDrag = false;
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
                if (amount > 0 && GetDistance() > 2)
                {
                    //drop half when pressing shift and 1 when not
                    int a = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? (amount / 2) : 1;
                    itemPlacingHandler.TryPlace(type, rectTransform.position, amount:a); //try place single item

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

        if (distance > 2)
        {
            if (itemPlacingHandler.IsAboveReceiver())
            {
                EnableVisuals();
                itemPlacingHandler.Hide();
                bool canDrop = itemPlacingHandler.WouldBeReceived();
                canDropOverlay.SetActive(canDrop);
                canNotDropOverlay.SetActive(!canDrop);
            }
            else
            {
                if (VisualsEnabled)
                {
                    itemPlacingHandler.Show(new ItemAmountPair(type, amount), origin);
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

    public class Factory : Zenject.PlaceholderFactory<UnityEngine.Object, InventorySlotVisualizer>
    {
    }
}
