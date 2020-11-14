using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewOrderUIElement : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Text text;
    [SerializeField] ItemType elementType;
    [SerializeField] Color color_zero, color_moreThanZero;

    int amount = 0;
    NewOrderVisualizer order;

    private void Start()
    {
        UpdateText();
        order = GetComponentInParent<NewOrderVisualizer>();
    }

    private void UpdateText()
    {
        text.text = amount + " x " + elementType.ToString();
        text.color = (amount > 0) ? color_moreThanZero : color_zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            Decrease();
    }

    public void Increase()
    {
        amount = Mathf.Clamp(amount + 1, 0, 999);
        order?.UpdateAmount(elementType, amount);
        UpdateText();
    }

    public void Decrease()
    {
        amount = Mathf.Clamp(amount - 1, 0, 999);
        order?.UpdateAmount(elementType, amount);
        UpdateText();
    }
}
