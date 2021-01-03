using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogVisualizer : MonoBehaviour, IDialogVisualizer
{
    [Zenject.Inject] AltarDialogHandler dialogHandler;

    public enum State { None, Display, Selection }

    private State state = State.None;
    private event System.Action<int> selectionEvent;
    private string text;
    private string[] options;

    void Start()
    {
        dialogHandler.SetVisualizer(this);
    }

    public void DisplayOptions(string[] options)
    {
        state = State.Selection;
        this.options = options;
    }

    public void DisplaySentence(string message)
    {
        state = State.Display;
        text = message;
    }

    public void SubscribeToSelection(Action<int> a)
    {
        selectionEvent += a;
    }

    public void UnsubscribeFromSelection(Action<int> a)
    {
        selectionEvent -= a;
    }

    private void OnGUI()
    {
        if (state == State.Display)
        {
            GUI.Label(new Rect(10, 10, 500, 500), text);
        }
        else if (state == State.Selection)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (GUI.Button(new Rect(10, 10 + i*50, 200, 40), options[i]))
                {
                    state = State.None;
                    selectionEvent?.Invoke(i);
                }
            }
        }
    }
}
