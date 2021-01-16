using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : StateListenerBehaviour
{
    [SerializeField] SceneReference mainMenu;
    [SerializeField] Image darkoverlay;

    public event System.Action PauseEnter, PauseExit;

    protected override void OnRealStart()
    {
        Unpause();
    }

    public void TogglePause()
    {

        if (gameState.CurrentState == GameState.State.Paused)
        {
            Unpause();
        }
        else if (gameState.CurrentState == GameState.State.Playing)
        {
            Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        darkoverlay.color = new Color(0, 0, 0, 0.66f);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        gameState.ChangeStateTo(GameState.State.Paused);
        PauseEnter?.Invoke();
    }

    public void Unpause()
    {
        Time.timeScale = 1;

        darkoverlay.color = new Color(0, 0, 0, 0);

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        gameState.ChangeStateTo(GameState.State.Playing);
        PauseExit?.Invoke();
    }

    public void ReturnToMainMenu()
    {
        Unpause();
        SceneManager.LoadScene(mainMenu);
    }
}