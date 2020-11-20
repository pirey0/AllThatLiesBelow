using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateListenerBehaviour : MonoBehaviour
{

    protected virtual void OnEnable()
    {
        GameState.Instance.StateChanged += OnStateChanged;
    }

    protected virtual void OnDisable()
    {
        GameState.Instance.StateChanged -= OnStateChanged;
    }


    protected virtual void OnStateChanged(GameState.State newState)
    {

    }
}
