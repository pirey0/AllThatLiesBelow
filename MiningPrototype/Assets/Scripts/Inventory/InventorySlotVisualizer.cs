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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null)
            return;

        rectTransform.position = eventData.pointerCurrentRaycast.worldPosition;

        Vector2 distance = eventData.position - Util.ScreenCenter;



    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inUI)
        {
            rectTransform.anchoredPosition = defaultAnchorPosition;
        }
        else
        {
            rectTransform.anchoredPosition = defaultAnchorPosition;
            Debug.Log("Dropped outside UI");
        }
    }


}
