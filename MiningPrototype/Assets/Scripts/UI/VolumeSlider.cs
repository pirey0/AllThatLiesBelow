using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider slider;

    private void Start()
    {
        if (mixer.GetFloat("MasterVolume", out float vol))
        {
            slider.value = DBTo01(vol);
        }
    }

    public void SetVolume(float s01)
    {
        mixer.SetFloat("MasterVolume", SliderToDB(s01));
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
