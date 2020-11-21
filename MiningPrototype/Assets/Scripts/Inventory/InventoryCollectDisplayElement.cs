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
    [SerializeField] float moveSpeed;
    [SerializeField] AnimationCurve fadeCurve;
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
        var info = ItemsData.GetItemInfo(obj.type);

        text.text = "+" + (info.AmountIsUniqueID ? 1 : obj.amount);
        icon.sprite = ItemsData.GetSpriteByItemType(obj);
    }

    private IEnumerator FadeOut()
    {
        float t = 0;

        while (t < duration)
        {
            Color c = new Color(text.color.r, text.color.g, text.color.b, fadeCurve.Evaluate(t / duration));
            text.color = c;
            icon.color = c;
            transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

            yield return null;
            t += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
