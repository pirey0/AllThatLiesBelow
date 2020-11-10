using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarDialogVisualizer : MonoBehaviour
{
    [SerializeField] Vector3 leftSpawnPosition, rightSpawnposition;
    [SerializeField] Vector3 offsetWithEveryWord;
    [SerializeField] DialogElementVisualization dialogOptionPrefab, dialogCommentPrefab;
    DialogElementVisualization[] dialogOptions = new DialogElementVisualization[3];

    [SerializeField] GameObject ray1, ray2, particleSystem;

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
    }

    [Button]
    public void DisplaySentence()
    {
        string[] str = new string[4] { "want?", "you", "do", "What" };
        StartCoroutine(DisplayWords(str,2,0.5f,1.5f));
    }

    [Button]
    public void DisplayOptions()
    {
        dialogOptions[0] = PrintOption("Option1", Vector3.zero, 2);
        dialogOptions[1] = PrintOption("Option2", Vector3.down, 2);
        dialogOptions[2] = PrintOption("Option3", Vector3.down * 2, 2);
    }

    IEnumerator DisplayWords(string[] words ,int wordsPerLine, float waitTimeBetweenWords, float waitTimeAfterLinebreak)
    {
        int wordsInLine = 0;
        int wordsInGeneral = 0;
        float lifetimeDifferencePerWord = words.Length * (waitTimeBetweenWords + waitTimeAfterLinebreak / wordsPerLine);

        foreach (string word in words)
        {
            float lerp = (float)wordsInLine / (float)(wordsPerLine-1);
            PrintWord(word, Vector3.Lerp(leftSpawnPosition,rightSpawnposition,lerp) + offsetWithEveryWord * wordsInGeneral, 2 + (1 - (wordsInGeneral / words.Length)) * lifetimeDifferencePerWord);

            wordsInLine++;
            wordsInGeneral++;

            if (wordsInLine >= wordsPerLine)
            {
                wordsInLine = 0;
                wordsInGeneral = 0;
                yield return new WaitForSeconds(waitTimeAfterLinebreak);
            }
            else
                yield return new WaitForSeconds(waitTimeBetweenWords);
        }
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
                Debug.Log("clicked on option " + (i+1));
                for (int j = dialogOptions.Length - 1; j >= 0; j--)
                {
                    Destroy(dialogOptions[j].gameObject);
                }
            }
        }
    }
}
