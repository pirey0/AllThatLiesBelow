using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using UnityEngine;

public class AltarDialogVisualizer : MonoBehaviour
{
    [SerializeField] Vector3 leftSpawnPosition, rightSpawnposition;
    [SerializeField] Vector3 offsetWithRow;
    [SerializeField] float wordLengthOffsetMultiplier = 1f;
    [SerializeField] DialogElementVisualization dialogOptionPrefab, dialogCommentPrefab;
    DialogElementVisualization[] dialogOptions = new DialogElementVisualization[3];

    [SerializeField] GameObject ray1, ray2, particleSystem;
    public event System.Action<int> Progressed;

    [Button]
    public void StartDialog()
    {
        ray1.SetActive(true);
        ray2.SetActive(true);
        particleSystem.SetActive(true);
    }

    [Button]
    public void EndDialog()
    {
        ray1.SetActive(false);
        ray2.SetActive(false);
        particleSystem.SetActive(false);

        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    [Button]
    public void DisplaySentence(string sentence)
    {
        string[] words = sentence.Split(' ');
        words = words.ToArray();

        StartCoroutine(DisplayWords(words,2,0.25f,0.5f, 5));
    }

    [Button]
    public void DisplayOptions(string[] options)
    {
        dialogOptions = new DialogElementVisualization[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            dialogOptions[i] = PrintOption(options[i],Vector3.up * 2 + Vector3.down * 0.75f * i, 2);
        }
    }

    IEnumerator DisplayWords(string[] words ,int wordsPerLine, float waitTimeBetweenWords, float waitTimeAfterLinebreak, float waitTileAfterSentence)
    {
        int wordsInLine = 0;
        int wordsInGeneral = 0;
        float lifetimeDifferencePerWord = words.Length * (waitTimeBetweenWords + waitTimeAfterLinebreak / wordsPerLine);

        foreach (string word in words)
        {
            float lerp = (float)wordsInLine / (float)(wordsPerLine-1);
            int row = Mathf.FloorToInt((float)wordsInGeneral / (float)wordsPerLine);
            Debug.LogWarning(row);

            float offset = ((lerp > 0.5f ? 1f : -1f) * ((float)word.Length / 5f)) * wordLengthOffsetMultiplier;
            Debug.LogWarning(word + " => " +  offset);
            Vector3 leftRight = Vector3.Lerp(leftSpawnPosition, rightSpawnposition, lerp) + Vector3.right * offset;
            Vector3 upDown = offsetWithRow * (row + 0.1f * wordsInGeneral);

            PrintWord(word, leftRight + upDown, words.Length + (1 - (wordsInGeneral / words.Length)) * lifetimeDifferencePerWord);

            wordsInLine++;
            wordsInGeneral++;

            if (wordsInLine >= wordsPerLine)
            {
                wordsInLine = 0;
                yield return new WaitForSeconds(waitTimeAfterLinebreak);
            }
            else
                yield return new WaitForSeconds(waitTimeBetweenWords);
        }

        yield return new WaitForSeconds(waitTileAfterSentence);

        Progressed?.Invoke(0);
    }

    private void PrintWord (string textToPrint, Vector3 positionOffset, float duration = 5f)
    {
        Instantiate(dialogCommentPrefab, transform.position + positionOffset, Quaternion.identity, transform).Init(this,textToPrint, duration);
    }

    private DialogElementVisualization PrintOption(string textToPrint, Vector3 positionOffset, float duration = 5f)
    {
        return Instantiate(dialogOptionPrefab, transform.position + positionOffset, Quaternion.identity, transform).Init(this,textToPrint, duration);
    }

    public void InteractedWith(DialogElementVisualization dialogElementVisualization)
    {
        for (int i = 0; i < dialogOptions.Length; i++)
        {
            if (dialogOptions[i] != null && dialogOptions[i] == dialogElementVisualization)
            {
                for (int j = dialogOptions.Length - 1; j >= 0; j--)
                {
                    Destroy(dialogOptions[j].gameObject);
                }

                Debug.Log("clicked on option " + (i));
                Progressed?.Invoke(i);
            }
        }
    }
}
