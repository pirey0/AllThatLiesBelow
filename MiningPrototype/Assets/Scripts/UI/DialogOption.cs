using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogOption : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Image answerArrow, commentArrow;
    DialogVisualizer dialogVisualizer;
    int index = 0;

    public void Init(string text, DialogVisualizer dialogVisualizer)
    {
        this.text.text = text;
        this.dialogVisualizer = dialogVisualizer;
        answerArrow.enabled = false;
        commentArrow.enabled = true;
    }

    public void Init(string text, int index, DialogVisualizer dialogVisualizer)
    {
        this.text.text = text;
        this.dialogVisualizer = dialogVisualizer;
        this.index = index;
        answerArrow.enabled = true;
        commentArrow.enabled = false;
    }

    public void Interact()
    {
        if (dialogVisualizer != null)
            dialogVisualizer.OnSelectOption(index);
    }

    
}
