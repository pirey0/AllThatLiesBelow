using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotVisualizer : Button, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] Image icon;
    [SerializeField] Text amountDisplay;

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
    }
    public void SetButtonToSlot(UnityAction action)
    {
        onClick.AddListener(action);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        defaultAnchorPosition = rectTransform.anchoredPosition;
        inDrag = true;
        StartCoroutine(UpdatePosition());
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = Util.MouseToWorld(CameraController.Instance.Camera);
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
            rectTransform.position = Util.MouseToWorld(CameraController.Instance.Camera);
            UpdatePlacingPreview();
            yield return null;
        }
    }

    private void UpdatePlacingPreview()
    {
        ItemInfo info = ItemsData.GetItemInfo(type);
        if (!info.CanBePlaced)
            return;

        Vector2 distance = rectTransform.position - rectTransform.parent.position;
        if (distance.magnitude > 2)
        {
            if (inUI)
            {
                ItemPlacingHandler.Instance?.Show(type);
                inUI = false;
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

}
