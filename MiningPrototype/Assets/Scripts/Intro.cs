using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    public List<IntroTextElement> texts = new List<IntroTextElement>();
    [SerializeField] DialogElementVisualization textPrefab;
    [SerializeField] Canvas textCanvas;
    [SerializeField] RectTransform textSpawnPosition;
    [SerializeField] CameraController cameraController;
    [SerializeField] Transform playerBefore;
    [SerializeField] Transform playerPrefab;
    [SerializeField] float replacePlayerDelay;

    private void Start()
    {
        foreach (IntroTextElement element in texts)
        {
            StartCoroutine(DisplayTextDelayed(element));
        }

        Invoke("ReplacePlayer", replacePlayerDelay);
    }

    IEnumerator DisplayTextDelayed(IntroTextElement element)
    {
        yield return new WaitForSeconds(element.time);
        DialogElementVisualization text = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform);
        text.Init(null,element.text,element.duration);
    }

    private void ReplacePlayer ()
    {
        //Transform player = Instantiate(playerPrefab);
        //player.transform.position = playerBefore.position;
        //cameraController.FollowNewTarget(player);
        //Destroy(playerBefore.gameObject);
    }
}

[System.Serializable]
public class IntroTextElement
{
    public float time;
    public string text;
    public float duration;
}
