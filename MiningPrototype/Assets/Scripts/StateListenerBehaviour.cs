using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateListenerBehaviour : MonoBehaviour
{
    protected GameState gameState;

    [Zenject.Inject]
    private void InjectGameState(GameState gameState = null)
    {
        this.gameState = gameState;
        if (gameState != null)
            gameState.StateChanged += OnStateChangedInternal;
    }

    protected virtual void OnDisable()
    {
        if (gameState != null)
            gameState.StateChanged -= OnStateChangedInternal;
    }


    private void OnStateChangedInternal(GameState.State newState)
    {
        switch (newState)
        {
            case GameState.State.NewGame:
                OnNewGame();
                break;

            case GameState.State.FullStart:
                OnRealStart();
                break;

            case GameState.State.PostLoadScenes:
                OnPostSceneLoad();
                break;

            case GameState.State.PostLoadFromFile:
                OnPostLoadFromFile();
                break;

            case GameState.State.Paused:
                OnPaused();
                break;
        }

        OnStateChanged(newState);
    }

    protected virtual void OnPaused()
    {
    }

    protected virtual void OnStateChanged(GameState.State newState)
    {
    }

    protected virtual void OnNewGame()
    {
    }

    protected virtual void OnPostLoadFromFile()
    {

    }

    protected virtual void OnPostSceneLoad()
    {
    }

    protected virtual void OnRealStart()
    {
    }

}
