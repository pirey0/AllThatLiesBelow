using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SceneReference playScene;
    [SerializeField] UnityEngine.UI.Button loadButton;
    [SerializeField] Transform targetTransform;
    [SerializeField] float cameraSpeed, cameraDuration;
    [SerializeField] TMPro.TMP_Text loadText;
    [SerializeField] AnimationCurve FadeInYPosition;
    [SerializeField] AudioSource walkingAudio;

    private void Start()
    {
        SaveHandler.LoadFromSavefile = false;

        if (!SaveHandler.SaveFileExists())
        {
            loadButton.interactable = false;
        }
        else
        {
            var stats = SaveHandler.LoadStatsOnly();
            if (stats != null)
            {
                loadText.text = "Day " + stats.Day + " - " + stats.GetFormattedTimePlayed();
            }
            else
            {
                loadText.text = "---Error---";
            }
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
            targetTransform.localPosition = new Vector3(targetTransform.localPosition.x, FadeInYPosition.Evaluate(i), targetTransform.localPosition.z);
            walkingAudio.volume = Mathf.Clamp(i - (cameraDuration - 3f),0f,1f);
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
