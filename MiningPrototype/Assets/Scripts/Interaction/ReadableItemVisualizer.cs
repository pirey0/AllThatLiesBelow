using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadableItemVisualizer : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject familyPhoto;
    //[SerializeField] float width = 5;
    //[SerializeField] float numberOfCharactersPerHeightUnit = 1;
    public void DisplayText(Transform objSpawnedFrom, ReadableItem readable, bool showFamilyPhoto = false)
    {
        //set text
        text.text = readable.text;


        //adapt size
        //(transform as RectTransform).sizeDelta = new Vector2(width,textToDisplay.Length * numberOfCharactersPerHeightUnit);

        //familyPhoto
        if (showFamilyPhoto)
            familyPhoto.SetActive(true);
    }

    public void Hide()
    {
        ReadableItemHandler.Instance.Hide();
        Destroy(gameObject);
    }
}
