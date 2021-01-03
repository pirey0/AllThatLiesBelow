using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarDialogHandler : MonoBehaviour, IDialogVisualizer
{
    private IDialogVisualizer currentVisualizer;

    private event System.Action<int> selectionEvent;


    public void SetVisualizer(IDialogVisualizer newVisualizer)
    {
        if (currentVisualizer != null)
            currentVisualizer.UnsubscribeFromSelection(OnSelect);

        currentVisualizer = newVisualizer;

        if (newVisualizer != null)
            newVisualizer.SubscribeToSelection(OnSelect);
    }

    public void ClearVisualizer()
    {
        SetVisualizer(null);
    }

    private void OnSelect(int i)
    {
        selectionEvent?.Invoke(i);
    }

    public void DisplayOptions(string[] options)
    {
        if (currentVisualizer != null)
            currentVisualizer.DisplayOptions(options);
        else
            Debug.LogError("No Visualizer set");
    }

    public void DisplaySentence(string message)
    {
        if (currentVisualizer != null)
            currentVisualizer.DisplaySentence(message);
        else
            Debug.LogError("No Visualizer set");
    }

    public void SubscribeToSelection(Action<int> a)
    {
        selectionEvent += a;
    }

    public void UnsubscribeFromSelection(Action<int> a)
    {
        selectionEvent -= a;
    }

    public void StartDialog()
    {
        currentVisualizer.StartDialog();
    }

    public void EndDialog()
    {
        currentVisualizer.EndDialog();
    }
}
