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
    [SerializeField] AnimationCurve introYCameraPan, shortYPan;
    [SerializeField] AudioSource walkingAudio;

    [Zenject.Inject] GameInstanceDataManger gameInstanceDataManger;

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

        if (gameInstanceDataManger != null && gameInstanceDataManger.ReturnToMenuFromScene)
        {
            animator.SetTrigger("ReturnToMenu");
            StartCoroutine(CameraMovement(shortYPan, shortYPan.keys[shortYPan.length-1].time));
        }
        else
        {
            animator.SetTrigger("EnterFromStartup");
            StartCoroutine(CameraMovement(introYCameraPan, cameraDuration, delayWalkingSounds: true));
        }
    }

    public void MenueAction(string coroutineName)
    {
        StartCoroutine(coroutineName);
    }

    private IEnumerator CameraMovement(AnimationCurve curve, float duration, bool delayWalkingSounds = false)
    {
        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            targetTransform.localPosition = new Vector3(targetTransform.localPosition.x, curve.Evaluate(i), targetTransform.localPosition.z);

            if (delayWalkingSounds)
                walkingAudio.volume = Mathf.Clamp(i - (duration - 3f),0f,1f);

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
