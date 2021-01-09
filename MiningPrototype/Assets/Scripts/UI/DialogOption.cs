using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogOption : MonoBehaviour
{
    [SerializeField] TMP_Text textDisplay;
    [SerializeField] Image answerArrow, commentArrow;

    string text = "";
    DialogVisualizer dialogVisualizer;
    int index = 0;

    public void Init(string text, DialogVisualizer dialogVisualizer)
    {
        this.text = text;
        this.dialogVisualizer = dialogVisualizer;

        StartCoroutine(ShowTextDelayed(isOption: false));
    }

    public void Init(string text, int index, DialogVisualizer dialogVisualizer)
    {
        this.text = text;
        this.dialogVisualizer = dialogVisualizer;

        this.index = index;

        StartCoroutine(ShowTextDelayed(isOption: true));
    }

    IEnumerator ShowTextDelayed(bool isOption)
    {
        if (isOption)
        {
            textDisplay.text = text;

            for (int i = 0; i <= 5; i++)
            {
                textDisplay.rectTransform.localScale = Vector3.one * (float)i / 5f;
                yield return new WaitForSeconds(0.03f);
            }
        }
        else
        {
            for (int i = 0; i < text.Length; i++)
            {
                textDisplay.text = text.Substring(0, i) + "<alpha=#00>" + text.Substring(i, text.Length-i);
                yield return new WaitForSeconds(0.03f);
            }

            textDisplay.text = text;
        }

        (isOption ? answerArrow : commentArrow).enabled = true;
    }

    private void GetSpaces(int count)
    {
    }

    public void Interact()
    {
        if (dialogVisualizer != null)
            dialogVisualizer.OnSelectOption(index);
    }


}
