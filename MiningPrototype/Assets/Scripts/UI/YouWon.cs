﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YouWon : MonoBehaviour
{
    [SerializeField] AnimationCurve openCloseCurve;
    [SerializeField] RectTransform toScale;

    private void Start()
    {
        StartCoroutine(ScaleCoroutine(true));
    }

    IEnumerator ScaleCoroutine(bool scaleUp)
    {
        float timeMin = openCloseCurve.keys[0].time;
        float timeMax = openCloseCurve.keys[openCloseCurve.length - 1].time;
        float time = (scaleUp ? timeMin : timeMax);

        while (scaleUp && time < timeMax || !scaleUp && time > timeMin)
        {
            time += (scaleUp ? 1 : -1) * Time.deltaTime;
            toScale.localScale = Vector3.one * openCloseCurve.Evaluate(time);
            yield return null;
        }


        if (!scaleUp)
            Destroy(gameObject);
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine(false));
    }
}
