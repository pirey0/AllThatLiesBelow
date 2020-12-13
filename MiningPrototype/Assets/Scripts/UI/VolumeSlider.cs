using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    const string VOLUME_KEY = "MasterVolume";
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider slider;

    private void Start()
    {
        if (PlayerPrefs.HasKey(VOLUME_KEY))
        {
            float val = PlayerPrefs.GetFloat(VOLUME_KEY);
            slider.value = val;
            mixer.SetFloat(VOLUME_KEY, SliderToDB(val));
        }
        else
        {
            slider.value = 1;
            mixer.SetFloat(VOLUME_KEY, SliderToDB(1));
            PlayerPrefs.SetFloat(VOLUME_KEY, 1);
        }
    }

    public void SetVolume(float s01)
    {
        mixer.SetFloat(VOLUME_KEY, SliderToDB(s01));
        PlayerPrefs.SetFloat(VOLUME_KEY, s01);
    }

    // See https://sound.stackexchange.com/questions/38722/convert-db-value-to-linear-scale
    float DBTo01(float db)
    {
        return Mathf.Pow(10, db / 20);
    }

    float SliderToDB(float v01)
    {
        if (v01 == 0) //avoid Log(0) =1
            return -80;

        return 20 * Mathf.Log10(v01);
    }
}
