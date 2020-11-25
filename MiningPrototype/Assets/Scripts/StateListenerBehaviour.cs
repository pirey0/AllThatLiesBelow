using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateListenerBehaviour : MonoBehaviour
{
    [Zenject.Inject] protected GameState gameState;

    protected virtual void OnEnable()
    {
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
                OnStartAfterLoad();
                break;

            case GameState.State.PostLoadScenes:
                OnPostSceneLoad();
                break;

            case GameState.State.PostLoadFromFile:
                OnPostLoadFromFile();
                break;
        }

        OnStateChanged(newState);
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

    protected virtual void OnStartAfterLoad()
    {
    }

}
