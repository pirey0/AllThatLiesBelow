using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Timeline;

public class InventorySlotVisualizer : Button, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] public Image icon;
    [SerializeField] public TMP_Text amountDisplay;
    [SerializeField] AnimationCurve scaleOnOpenAndCloseCurve;

    int amount;
    ItemType type;
    RectTransform rectTransform;
    Vector2 defaultAnchorPosition;

    bool inUI;
    Coroutine updateRoutine;
    bool inDrag;

    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Display(KeyValuePair<ItemType, int> pair)
    {
        amount = pair.Value;
        type = pair.Key;

        if (icon != null)
            icon.sprite = ItemsData.GetSpriteByItemType(type);

        if (amountDisplay != null)
            amountDisplay.text = amount.ToString();

        StartCoroutine(ScaleCoroutine(scaleUp: true));
    }
    public void SetButtonToSlot(UnityAction action)
    {
        onClick.AddListener(action);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        var info = ItemsData.GetItemInfo(type);
        TooltipHandler.Instance?.Display(transform, info.DisplayName, info.DisplayTooltip);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        TooltipHandler.Instance?.StopDisplaying(transform);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        defaultAnchorPosition = rectTransform.anchoredPosition;
        inDrag = true;
        StartCoroutine(UpdatePosition());
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Util.MouseToWorld();
        UpdatePlacingPreview();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!inUI)
        {
            Debug.Log("Dropped outside UI");
            ItemPlacingHandler.Instance?.TryPlace(type, rectTransform.position);
        }
        ItemPlacingHandler.Instance?.Hide();
        inDrag = false;
        EnableVisuals();
        rectTransform.anchoredPosition = defaultAnchorPosition;
    }

    //to also update when not moving the mouse
    private IEnumerator UpdatePosition()
    {
        while (inDrag)
        {
            rectTransform.position = Util.MouseToWorld();
            UpdatePlacingPreview();
            yield return null;
        }
    }

    private void UpdatePlacingPreview()
    {
        Vector2 distance = rectTransform.position - rectTransform.parent.position;
        var info = ItemsData.GetItemInfo(type);

        if (distance.magnitude > 2)
        {
            if (inUI)
            {
                ItemPlacingHandler.Instance?.Show(new ItemAmountPair(type, amount));
                inUI = false;
                if (info.CanBePlaced)
                    DisableVisuals();
            }
            ItemPlacingHandler.Instance?.UpdatePosition(rectTransform.position);
        }
        else
        {
            if (!inUI)
            {
                ItemPlacingHandler.Instance?.Hide();
                inUI = true;
                EnableVisuals();
            }
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
        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine(scaleUp: false));
    }

    IEnumerator ScaleCoroutine(bool scaleUp)
    {
        Debug.Log("started scale: up?" +scaleUp);
        float timeMin = scaleOnOpenAndCloseCurve.keys[0].time;
        float timeMax = scaleOnOpenAndCloseCurve.keys[scaleOnOpenAndCloseCurve.length - 1].time;
        float time = (scaleUp ? timeMin : timeMax);

        while (scaleUp && time < timeMax || !scaleUp && time > timeMin)
        {
            time += (scaleUp ? 1 : -1) * Time.deltaTime;
            transform.localScale = Vector3.one * scaleOnOpenAndCloseCurve.Evaluate(time);
            yield return null;
        }

        Debug.Log("finished scale: up?" + scaleUp);

        if (!scaleUp)
            Destroy(gameObject);
    }

}
