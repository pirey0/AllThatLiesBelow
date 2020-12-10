using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    public List<IntroTextElement> texts = new List<IntroTextElement>();
    [SerializeField] DialogElementVisualization textPrefab;
    [SerializeField] Canvas textCanvas;
    [SerializeField] RectTransform textSpawnPosition;

    private void Start()
    {
        foreach (IntroTextElement element in texts)
        {
            StartCoroutine(DisplayTextDelayed(element));
        }
    }

    IEnumerator DisplayTextDelayed(IntroTextElement element)
    {
        yield return new WaitForSeconds(element.time);
        DialogElementVisualization text = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform);
        text.Init(null,element.text,element.duration);
    }
}

[System.Serializable]
public class IntroTextElement
{
    public float time;
    public string text;
    public float duration;
}
