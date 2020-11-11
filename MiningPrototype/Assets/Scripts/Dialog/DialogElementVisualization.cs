using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogElementVisualization : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] AnimationCurve alphaOverTime;
    [SerializeField] AnimationCurve heightOverTime;
    [SerializeField] AnimationCurve amountOfCharactersOverTime;
    [SerializeField] AnimationCurve pivotXOffsetOverTime;
    [SerializeField] AnimationCurve pivotYOffsetOverTime;
    [SerializeField] AnimationCurve pivotVarianceOverTime;

    [Range(0f,1f)]
    [SerializeField] float pivotOffsetChangeSpeedVariance = 0.1f;
    Vector3 positionInTheBeginning;

    [SerializeField] float displayTime;
    [SerializeField] bool dieAtTheEnd = true;
    [SerializeField] bool isInteractable = false;

    AltarDialogVisualizer altar;

    public DialogElementVisualization Init(AltarDialogVisualizer _altar, string textToPrint, float duration)
    {
        positionInTheBeginning = transform.position;
        displayTime = duration;
        StartCoroutine(PrintTextDelayed(textToPrint));
        altar = _altar;
        return this;
    }

    IEnumerator PrintTextDelayed(string textToPrint)
    {
        float alphaCurveLength = alphaOverTime.keys[alphaOverTime.length - 1].time;
        float heightCurveLength = heightOverTime.keys[heightOverTime.length - 1].time;
        float charactersLength = amountOfCharactersOverTime.keys[amountOfCharactersOverTime.length - 1].time;
        float pivotVarianceLength = pivotVarianceOverTime.keys[pivotVarianceOverTime.length - 1].time;

        float pivotXOffsetOffsetTime = Random.value * 10f;
        float pivotYOffsetOffsetTime = Random.value * 10f;
        float pivotXOffsetVarianceSpeed = Random.Range(1 - pivotOffsetChangeSpeedVariance, 1 + pivotOffsetChangeSpeedVariance);
        float pivotYOffsetVarianceSpeed = Random.Range(1 - pivotOffsetChangeSpeedVariance, 1 + pivotOffsetChangeSpeedVariance);
        //bool pivotXisReadBackwards = Random.value > 0.5f;

        float t = 0;

        while (t < displayTime)
        {
            float progress = (t / displayTime);

            int amountOfCharactersToShow =  (int)(Mathf.Clamp(amountOfCharactersOverTime.Evaluate(progress * charactersLength),0,1) * textToPrint.Length);
            text.text = textToPrint.Substring(0, amountOfCharactersToShow);

            float alpha = alphaOverTime.Evaluate(progress * alphaCurveLength);
            text.color = new Color(1, 1, 1, alpha);

            transform.position = positionInTheBeginning + Vector3.up * heightOverTime.Evaluate(progress * heightCurveLength);

            float pivotXOffset = pivotXOffsetOverTime.Evaluate(pivotXOffsetOffsetTime + t * pivotXOffsetVarianceSpeed) * pivotVarianceOverTime.Evaluate(progress * pivotVarianceLength);
            float pivotYOffset = pivotYOffsetOverTime.Evaluate(pivotYOffsetOffsetTime + t * pivotYOffsetVarianceSpeed) * pivotVarianceOverTime.Evaluate(progress * pivotVarianceLength);
            (transform as RectTransform).pivot = new Vector2(0.5f + pivotXOffset, 0.5f + pivotYOffset);

            t += Time.deltaTime;
            yield return null;
        }

        if (dieAtTheEnd)
        {
            Destroy(gameObject);
        }    
    }

    public void Destroy()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        for (int i = 100; i > 0; i--)
        {
            text.color = new Color(1,1,1,((float)i/100));
            yield return null;
        }

        Destroy(gameObject);
    }

    public void OnMouseEnter()
    {
        if (isInteractable)
            text.fontStyle = FontStyles.Underline;
    }

    public void OnMouseExit()
    {
        if (isInteractable)
            text.fontStyle = FontStyles.Normal;
    }

    public void OnClick()
    {
        if (isInteractable)
            altar.InteractedWith(this);
    }
}
