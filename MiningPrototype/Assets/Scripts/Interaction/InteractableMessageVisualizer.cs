using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableMessageVisualizer : ScalingUIElementBase
{
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject familyPhoto;
    //[SerializeField] float width = 5;
    //[SerializeField] float numberOfCharactersPerHeightUnit = 1;
    public void DisplayText(Transform objSpawnedFrom, string textToDisplay, bool showFamilyPhoto = false)
    {
        //set text
        text.text = textToDisplay;

        //set postion
        transformToFollow = objSpawnedFrom;
        UpdatePosition();

        //adapt size
        //(transform as RectTransform).sizeDelta = new Vector2(width,textToDisplay.Length * numberOfCharactersPerHeightUnit);

        //familyPhoto
        if (showFamilyPhoto)
            familyPhoto.SetActive(true);

        //scaleUp
        StartCoroutine(ScaleCoroutine());
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine(scaleUp: false));
    }
}
