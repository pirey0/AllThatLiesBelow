using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SceneReference playScene;
    [SerializeField] UnityEngine.UI.Button loadButton;
    [SerializeField] Transform transform;
    [SerializeField] float cameraSpeed, cameraDuration;
    private void Start()
    {
        SaveHandler.LoadFromSavefile = false;

        if (!SaveHandler.SaveFileExists())
        {
            loadButton.interactable = false;
        }
        StartCoroutine(CameraMovement());
    }

    public void MenueAction(string coroutineName)
    {
        StartCoroutine(coroutineName);
    }

    private IEnumerator CameraMovement()
    {
        for (float i = 0; i < cameraDuration; i += Time.deltaTime)
        {
            transform.position += new Vector3(0, cameraSpeed * Time.deltaTime, 0);
            yield return null;
        }
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
        SaveHandler.LoadFromSavefile = true;
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
