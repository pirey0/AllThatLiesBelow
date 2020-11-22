using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SceneReference playScene;

    private void Start()
    {
        SaveHandler.LoadedFromSaveFile = false;
    }

    public void MenueAction(string coroutineName)
    {
        StartCoroutine(coroutineName);
    }

    private IEnumerator Play()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(playScene);
    }

    private IEnumerator Load()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.5f);
        SaveHandler.LoadedFromSaveFile = true;
        yield return Play();
    }

    private IEnumerator Settings()
    {
        yield return null;
    }

    private IEnumerator Quit()
    {
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
