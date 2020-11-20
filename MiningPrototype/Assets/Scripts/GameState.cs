using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    private static GameState instance;
    public static GameState Instance { get => instance; }

    private State currentState;
    public event System.Action<State> StateChanged;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Setup()
    {
        instance = new GameState();
    }

    public void ChangeStateTo(State state)
    {
        if (currentState != state)
        {
            Debug.Log("Gamestate changed to " + state);
            currentState = state;
            StateChanged?.Invoke(state);
            OnStateChanged();
        }
    }

    private void OnStateChanged()
    {
        if(currentState == State.Ready)
        {
            ChangeStateTo(State.Playing);
        }
    }

    public enum State
    {
        Loading,
        Ready,
        Playing,
        Respawning
    }
}

