using Microsoft.Unity.VisualStudio.Editor;
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

    [SerializeField] SpriteRenderer ray1, ray2;
    [SerializeField] GameObject particleSystem;
    [SerializeField] AnimationCurve opcacityVariance;
    [SerializeField] float opacityAdaptationSpeed = 4;
    float lightOpacityMultiplier = 0;
    float lightOpacityMultiplierTarget = 0;

    [SerializeField] AudioSource voicesAudio;
    [SerializeField] float voicesVolumeDecayMultiplier = 1f;
    [SerializeField] float voicesVolumeAdaptMultiplier = 1f;
    [SerializeField] float voiceVolumePerWordOrOption = 1f;
    float voicesVolumeCurrent;
    float voicesVolumeTarget;


    public event System.Action<int> Progressed;

    private void Update()
    {
        if (lightOpacityMultiplierTarget > lightOpacityMultiplier)
            lightOpacityMultiplier =  Mathf.Clamp(lightOpacityMultiplier + Time.deltaTime * opacityAdaptationSpeed, 0,1);
        else if (lightOpacityMultiplierTarget < lightOpacityMultiplier)
            lightOpacityMultiplier = Mathf.Clamp(lightOpacityMultiplier - Time.deltaTime * opacityAdaptationSpeed, 0, 1);

        Color c = new Color(1,1,1, opcacityVariance.Evaluate(Time.time) * lightOpacityMultiplier);
        ray1.color = c;
        ray2.color = c;

        voicesVolumeTarget = Mathf.Clamp(voicesVolumeTarget - Time.deltaTime * voicesVolumeDecayMultiplier,0,1);
        voicesVolumeCurrent = Mathf.MoveTowards(voicesVolumeCurrent, voicesVolumeTarget, Time.deltaTime * voicesVolumeAdaptMultiplier);

        voicesAudio.volume = voicesVolumeCurrent;

    }

    [Button]
    public void StartDialog()
    {
        lightOpacityMultiplierTarget = 1;
        particleSystem.SetActive(true);
    }

    [Button]
    public void EndDialog()
    {
        StopAllCoroutines();

        lightOpacityMultiplierTarget = 0;
        particleSystem.SetActive(false);

        foreach (Transform child in transform)
        {
            DialogElementVisualization dev = child.GetComponent<DialogElementVisualization>();
            if (dev != null)
                dev.Destroy();
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
        voicesVolumeTarget += voiceVolumePerWordOrOption;
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

            float offset = ((lerp > 0.5f ? 1f : -1f) * ((float)word.Length / 5f)) * wordLengthOffsetMultiplier;
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
        voicesVolumeTarget += voiceVolumePerWordOrOption;
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
