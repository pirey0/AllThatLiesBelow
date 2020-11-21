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
            slider.value = vol;
        }
    }

    public void SetVolume(float db)
    {
        mixer.SetFloat("MasterVolume", db);
    }


}
