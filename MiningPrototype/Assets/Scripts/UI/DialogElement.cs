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

    const float charactersPerLine = 20f;

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
        offset = newOffset;

        startPosition = transform.position;

        float h = 1;

        for (int i = 0; i < options.Length; i++)
        {
            InstantiateOption().Init(options[i], i, dialogVisualizer);
            h += GetHeightFromCharacterAmount(options[i].Length);
        }

        SetHeight(h);

        return this;
    }

    private float GetHeightFromCharacterAmount(int length)
    {
        return Mathf.Max(1, ((float)length / charactersPerLine) * 0.33f);
    }

    private Vector2 AdaptHeigthToCharacterAmount(int length)
    {
        if (length < 10)
            return new Vector2(3, 2);

        if (length > 20)
            return new Vector2(5, Mathf.Max(2, Mathf.Ceil(((float)length / charactersPerLine)*0.5f)));

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
    private void SetHeight(float height, float width = 4)
    {
        (transform as RectTransform).sizeDelta = new Vector2(width, Mathf.Max(height,2f));
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
