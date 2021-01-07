using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class DialogElement : MonoBehaviour
{
    [SerializeField] VerticalLayoutGroup optionsGroup;
    [SerializeField] DialogOption dialogOptionPrefab;
    [SerializeField] ImageSpriteAnimator imageSpriteAnimator;
    [SerializeField] SpriteAnimation hideAnimation;

    bool isChoice = false;
    Transform transformToFollow;
    Vector3 startPosition;
    Vector3 offset;

    public DialogElement Init(string str, DialogVisualizer dialogVisualizer, Vector3 newOffset)
    {
        InstantiateOption().Init(str, dialogVisualizer);

        startPosition = transform.position;
        offset = newOffset;

        (transform as RectTransform).sizeDelta = AdaptHeigthToCharacterAmount(str.Length);
        UpdatePosition();
        return this;
    }

    public DialogElement Init(string[] options, DialogVisualizer dialogVisualizer, Vector3 newOffset)
    {
        isChoice = true;
        offset = newOffset;

        startPosition = transform.position;

        for (int i = 0; i < options.Length; i++)
        {
            InstantiateOption().Init(options[i], i, dialogVisualizer);
        }

        SetHeight((1f + options.Length) / 1.5f);

        return this;
    }

    private Vector2 AdaptHeigthToCharacterAmount(int length)
    {
        if (length < 10)
            return new Vector2(3, 2);

        if (length > 20)
            return new Vector2(5, Mathf.Max(2, 1 + Mathf.CeilToInt((float)length / 40f)));

        return new Vector2(4, 2);
    }

    internal void StartFollowing(Transform transform, Vector3 newOffset)
    {
        transformToFollow = transform;
        offset = newOffset;
        StartCoroutine(FollowingRoutine());
    }

    IEnumerator FollowingRoutine()
    {
        while (this != null)
        {
            UpdatePosition();
            yield return null;
        }
    }

    private void UpdatePosition()
    {
        Vector3 pos = transformToFollow != null ? transformToFollow.position : startPosition;
        transform.position = pos + ((transform as RectTransform).sizeDelta.y / 2f) * Vector3.up + offset;
    }
    private void SetHeight(float height)
    {
        (transform as RectTransform).sizeDelta = new Vector2(4, Mathf.Max(height,2f));
    }

    private DialogOption InstantiateOption()
    {
        return Instantiate(dialogOptionPrefab, optionsGroup.transform);
    }

    public void Hide()
    {
        if (this == null)
            return;

        for (int i = optionsGroup.transform.childCount - 1; i >= 0; i--)
            Destroy(optionsGroup.transform.GetChild(i).gameObject);

        StopAllCoroutines();
        StartCoroutine(HideRoutine());
    }

    public IEnumerator HideRoutine()
    {
        imageSpriteAnimator.Play(hideAnimation);
        while (!imageSpriteAnimator.IsDone())
            yield return null;

        Destroy(gameObject);
    }
}
