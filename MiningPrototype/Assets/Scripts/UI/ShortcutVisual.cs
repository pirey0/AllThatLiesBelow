using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutVisual : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TMP_Text tMP_Text;

    internal void Init(Sprite displaySprite, KeyCode shortcut)
    {
        SetIconSprite(displaySprite);
        string text = shortcut.ToString();
        tMP_Text.text = text.Length > 1 ? text.Substring(text.Length - 1, 1) : text;
    }

    private void SetIconSprite(Sprite sprite)
    {
        (icon.transform as RectTransform).sizeDelta = new Vector2((float)sprite.rect.width / 16f, (float)sprite.rect.height / 16f);
        icon.sprite = sprite;
    }
}
