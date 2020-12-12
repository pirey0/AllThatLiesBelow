using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingDisplayer : StateListenerBehaviour
{
    [SerializeField] Image image;
    [SerializeField] float rotSpeed;
    [SerializeField] Slider slider;
    [SerializeField] TMPro.TMP_Text loadingText, text;

    [Zenject.Inject] SceneAdder sceneAdder;

    private void Start()
    {
        if (SaveHandler.LoadFromSavefile)
        {
            slider.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);
            text.text = "Loading";
        }
        else
        {
            text.text = "Generating";
            slider.minValue = 0;
            slider.maxValue = sceneAdder.LoadingTotal;
        }
    }

    private void Update()
    {
        if (!SaveHandler.LoadFromSavefile)
        {
            slider.value = sceneAdder.LoadingCurrent;
            loadingText.text = Mathf.CeilToInt(((float)sceneAdder.LoadingCurrent / sceneAdder.LoadingTotal) * 100).ToString() + "%";
        }

        image.transform.Rotate(0, 0, Time.unscaledDeltaTime * rotSpeed);
    }

    protected override void OnRealStart()
    {
        Destroy(gameObject);
    }
}
