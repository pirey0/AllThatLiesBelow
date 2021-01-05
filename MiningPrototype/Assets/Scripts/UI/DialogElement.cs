using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class DialogElement : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] VerticalLayoutGroup optionsGroup;
    [SerializeField] DialogOption dialogOptionPrefab;
    [SerializeField] ImageSpriteAnimator imageSpriteAnimator;
    [SerializeField] SpriteAnimation hideAnimation;

    bool isChoice = false;

    public DialogElement Init(string str)
    {
        text.text = str;
        return this;
    }

    internal DialogElement Init(string[] options, DialogVisualizer dialogVisualizer)
    {
        isChoice = true;
        for (int i = 0; i < options.Length; i++)
        {
            InstantiateOption().Init(options[i],i,dialogVisualizer);
        }

        SetHeight((1f+options.Length)/2);

        return this;
    }

    private void SetHeight(float height)
    {
        (transform as RectTransform).sizeDelta = new Vector2(4, height);
    }

    private DialogOption InstantiateOption()
    {
        return Instantiate(dialogOptionPrefab, optionsGroup.transform);
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideRoutine());
    }

    public IEnumerator HideRoutine()
    {
        text.text = "";
        imageSpriteAnimator.Play(hideAnimation);
        while (!imageSpriteAnimator.IsDone())
            yield return null;

        Destroy(gameObject);
    }
}
