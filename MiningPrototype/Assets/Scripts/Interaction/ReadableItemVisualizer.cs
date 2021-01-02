using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadableItemVisualizer : MonoBehaviour, ILayeredUI
{
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject familyPhoto;
    [SerializeField] ImageSpriteAnimator spriteAnimator;
    [SerializeField] Image xToClose;
    [SerializeField] SpriteAnimation letterCloseAnimation;
    [SerializeField] float openCloseMultiplier;
    [SerializeField] AnimationCurve textHeightoverTime;
    [SerializeField] Transform toScaleOnOpenAndClose;

    [Zenject.Inject] protected ReadableItemHandler readableItemHandler;
    [Zenject.Inject] UIsHandler uIsHandler;

    Coroutine close;
    [HideInInspector] public int id;

    protected virtual void Start()
    {
        uIsHandler.NotifyOpening(this);
    }

    //[SerializeField] float width = 5;
    //[SerializeField] float numberOfCharactersPerHeightUnit = 1;
    public void DisplayText(Transform objSpawnedFrom, ReadableItem readable, bool showFamilyPhoto = false)
    {
        //set text
        text.text = readable.text;

        StartCoroutine(ScaleCoroutine(scaleUp: true));

        //adapt size
        //(transform as RectTransform).sizeDelta = new Vector2(width,textToDisplay.Length * numberOfCharactersPerHeightUnit);

        //familyPhoto
        if (showFamilyPhoto)
            familyPhoto.SetActive(true);
    }

    protected IEnumerator ScaleCoroutine(bool scaleUp)
    {
        if (xToClose != null)
            xToClose.enabled = false;

        //Debug.Log("started scale: up?" + scaleUp);
        float timeMin = textHeightoverTime.keys[0].time;
        float timeMax = textHeightoverTime.keys[textHeightoverTime.length - 1].time;
        float time = (scaleUp ? timeMin : timeMax);

        while (scaleUp && time < timeMax || !scaleUp && time > timeMin)
        {
            time += (scaleUp ? 1 : -1) * Time.deltaTime * openCloseMultiplier;
            toScaleOnOpenAndClose.localScale = new Vector3(1, textHeightoverTime.Evaluate(time), 0);
            yield return null;
        }

        //Debug.Log("finished scale: up?" + scaleUp);


        if (xToClose != null)
            xToClose.enabled = scaleUp;

        if (!scaleUp)
            Destroy(gameObject);
    }

    public void Hide()
    {
        if (close != null)
            return;
        else
        {
            uIsHandler.NotifyClosing(this);
            StopAllCoroutines();
            close = StartCoroutine(ScaleCoroutine(scaleUp: false));
            spriteAnimator.Play(letterCloseAnimation);
            readableItemHandler.Hide();
        }
    }

    public void ForceClose()
    {
        Hide();
    }

    public class Factory : Zenject.PlaceholderFactory<UnityEngine.Object, ReadableItemVisualizer>
    {
    }
}
