using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuCreditsVisualizer : MonoBehaviour
{
    [SerializeField] TMP_Text bigText;
    [SerializeField] TMP_Text smallText;
    [SerializeField] TMP_Text specialText;
    [SerializeField] MainMenuCredits content;

    [SerializeField] AnimationCurve inOutCurve;
    [SerializeField] AudioSource woosh;

    float animationPosition = 0;
    int animationDirection = 0;

    bool shown;

    public void Show()
    {
        ApplyCreditContent();
        animationDirection = 1;
        shown = true;

        woosh.pitch = 1;
        woosh.Play();
    }

    public void Hide()
    {
        animationDirection = -1;
        shown = false;

        woosh.pitch = 0.66f;
        woosh.Play();
    }

    public void Toggle()
    {
        if (shown)
            Hide();
        else
            Show();
    }

    private void Update()
    {
        if (animationDirection != 0)
        {
            animationPosition += animationDirection * Time.deltaTime;
            RectTransform rectTransform = transform as RectTransform;
            rectTransform.anchoredPosition  = new Vector3(inOutCurve.Evaluate(animationPosition), rectTransform.anchoredPosition.y);

            if ((animationDirection > 0 && animationPosition >= inOutCurve.keys[inOutCurve.length - 1].time) || (animationDirection < 0 && animationPosition <= inOutCurve.keys[0].time))
                animationDirection = 0;
        }
    }

    private void ApplyCreditContent()
    {
        bigText.text = content.BigText;
        bigText.fontSize = content.BigTextSize;
        smallText.text = content.SmallText;
        smallText.fontSize = content.SmallTextSize;
        specialText.text = content.SpecialText;
        specialText.fontSize = content.SpecialTextSize;
    }
}
