using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCollectDisplayElement : MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] Text text;
    [SerializeField] Image icon;
    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }

    public void SetItem(ItemAmountPair obj)
    {
        text.text = "+" + obj.amount;
        icon.sprite = ItemsData.GetSpriteByItemType(obj.type);
    }

    private IEnumerator FadeOut()
    {
        float t = 0;

        while (t < duration)
        {
            Color c = new Color(text.color.r, text.color.g, text.color.b, 1 - t / duration);
            text.color = c;
            icon.color = c;

            yield return null;
            t += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
