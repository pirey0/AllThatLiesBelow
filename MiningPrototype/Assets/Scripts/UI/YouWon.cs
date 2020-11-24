using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class YouWon : MonoBehaviour
{
    [SerializeField] AnimationCurve openCloseCurve;

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
            transform.localScale = Vector3.one * openCloseCurve.Evaluate(time);
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
