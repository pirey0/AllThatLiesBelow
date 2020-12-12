using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    private State currentState = State.OutOfGame;
    private State oldState;
    public event System.Action<State> StateChanged;

    [Zenject.Inject] SaveHandler saveHandler;

    public State CurrentState { get => currentState; }

    public bool Playing { get => currentState == State.Playing || currentState == State.Respawning; }

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
                saveHandler.Load();
                ChangeStateTo(State.PostLoadFromFile);
                break;

            case State.PostLoadFromFile:
                ChangeStateNextFrameTo(State.FullStart);
                break;

            case State.PostLoadScenes:
                ChangeStateTo(State.NewGame);
                break;

            case State.NewGame:
                ChangeStateNextFrameTo(State.FullStart);
                break;
            case State.FullStart:
                ChangeStateTo(State.Playing);
                break;
        }
    }

    public void ChangeStateNextFrameTo(State ns)
    {
        StartCoroutine(DelayedChangeState(ns));
    }

    public void ReloadScene()
    {
        ChangeStateTo(State.OutOfGame);
        SaveHandler.LoadFromSavefile = SaveHandler.SaveFileExists();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator DelayedChangeState(State s)
    {
        yield return null;
        ChangeStateTo(s);
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
        Paused
    }
}

