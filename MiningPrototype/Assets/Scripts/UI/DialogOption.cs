using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogOption : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    DialogVisualizer dialogVisualizer;
    int index;

    public void Init(string text, int index, DialogVisualizer dialogVisualizer)
    {
        this.text.text = text;
        this.dialogVisualizer = dialogVisualizer;
        this.index = index;
    }

    public void Interact()
    {
        if (dialogVisualizer != null)
            dialogVisualizer.SelectOption(index);
    }
}
