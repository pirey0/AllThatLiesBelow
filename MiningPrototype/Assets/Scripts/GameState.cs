using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    private State currentState = State.OutOfGame;
    private State oldState;
    public event System.Action<State> StateChanged;

    public State CurrentState { get => currentState; }

    public bool Playing { get => currentState == State.Playing; }

    public void ChangeStateTo(State state)
    {
        if (currentState != state)
        {
            Debug.Log("Gamestate changed to " + state);
            oldState = currentState;
            currentState = state;
            StateChanged?.Invoke(state);
            OnStateChanged();
        }
    }

    public void ChangeToPrevious()
    {
        ChangeStateTo(oldState);
    }

    private void OnStateChanged()
    {
        switch (currentState)
        {
            case State.Entry:
                if (SaveHandler.LoadFromSavefile)
                    ChangeStateTo(State.PreLoadFromFile);
                else
                    ChangeStateTo(State.PreLoadScenes);
                break;

            case State.PreLoadFromFile:
                SaveHandler.Load();
                ChangeStateTo(State.PostLoadFromFile);
                break;

            case State.PostLoadFromFile:
                ChangeStateTo(State.FullStart);
                break;

            case State.PostLoadScenes:
                ChangeStateTo(State.NewGame);
                break;

            case State.NewGame:
                ChangeStateTo(State.FullStart);
                break;
            case State.FullStart:
                ChangeStateTo(State.Playing);
                break;
        }

    }

    public enum State
    {
        OutOfGame,
        Entry,
        PreLoadScenes,
        PostLoadScenes,
        NewGame,
        PreLoadFromFile,
        PostLoadFromFile,
        FullStart,
        Playing,
        Respawning,
    }
}

