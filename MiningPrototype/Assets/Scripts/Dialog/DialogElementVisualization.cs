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
    [SerializeField] AnimationCurve pivotVarianceOverTime;
    Vector3 positionInTheBeginning;

    [SerializeField] float displayTime;
    [SerializeField] bool dieAtTheEnd = true;
    [SerializeField] bool isInteractable = false;

    AltarDialogVisualizer altar;

    public DialogElementVisualization Init (AltarDialogVisualizer _altar,string textToPrint, float duration)
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

        float pivotDistance = 0f;

        float t = 0;

        while (t < displayTime)
        {
            float progress = (t / displayTime);

            int amountOfCharactersToShow =  (int)(Mathf.Clamp(amountOfCharactersOverTime.Evaluate(progress * charactersLength),0,1) * textToPrint.Length);
            text.text = textToPrint.Substring(0, amountOfCharactersToShow);

            float alpha = alphaOverTime.Evaluate(progress * alphaCurveLength);
            text.color = new Color(1, 1, 1, alpha);

            transform.position = positionInTheBeginning + Vector3.up * heightOverTime.Evaluate(progress * heightCurveLength);

            //RectTransform rectTransform = (transform as RectTransform);
            //float maxPivotDistance = pivotVarianceOverTime.Evaluate(progress * pivotVarianceLength) * Time.deltaTime;
            //float targetPivotOffset = 0.5f + Random.Range(0, maxPivotDistance);
            //float targetPivotSmoothed = Mathf.SmoothDamp(rectTransform.pivot.x, targetPivotOffset,ref pivotDistance,0.1f);
            //rectTransform.pivot = Vector3.one * targetPivotSmoothed;
            //pivotDistance = Mathf.Abs(rectTransform.pivot.x - targetPivotSmoothed);

            t += Time.deltaTime;
            yield return null;
        }

        if (dieAtTheEnd)
        {
            Destroy(gameObject);
        }    
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
