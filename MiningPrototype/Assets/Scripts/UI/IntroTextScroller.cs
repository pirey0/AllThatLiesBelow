using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class IntroTextScroller : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] [TextArea] string textToDisplay;
    [SerializeField] int range = 0;

    [SerializeField] int ToHighlight;
    int toHighlight;

    private void SetAlphaToHighlightTextAt(int center)
    {
        if (center == toHighlight)
            return;

        toHighlight = center;

        string str = "";
        int alphaBefore = 99;

        for (int i = 0; i < textToDisplay.Length; i++)
        {
            int alpha = (i > center && i < center + range)?99: Mathf.RoundToInt((1f - Mathf.Clamp(Mathf.Abs((float)center - (float)i) / (float)range, 0f, 1f)) * 100f);

            if (alpha != alphaBefore)
            {
                str += "<alpha=#" + GetDoubleStringAlpha(alpha) + ">";
                alphaBefore = alpha;
            }

            str += textToDisplay.Substring(i, 1);
        }

        text.text = str;
    }

    private string GetDoubleStringAlpha(int alpha)
    {
        return alpha < 10 ? "0" + alpha.ToString() : Mathf.Min(alpha,99).ToString();
    }

    private void Update()
    {
        SetAlphaToHighlightTextAt(ToHighlight);
    }
}
