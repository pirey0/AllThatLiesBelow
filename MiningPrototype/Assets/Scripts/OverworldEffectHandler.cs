using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEffectHandler : MonoBehaviour
{
    [SerializeField] float fadeHeight;
    [SerializeField] float fadeThickness;

    float alphaCalculatedBasedOnHeightOfPlayer;

    [SerializeField] SpriteRenderer vignetteRenderer;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] float amountOfParticles;

    [SerializeField] AudioSource audioSource;
    float audioSourceVolumeMultiplierThroughHut = 1;
    [SerializeField] Hut hut;

    void OnEnable()
    {
        hut.OnHutStateChange += OnHutStateChange;
    }


    void OnDisable()
    {
        hut.OnHutStateChange -= OnHutStateChange;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < fadeHeight - fadeThickness || transform.position.y > fadeHeight + fadeThickness)
            return;

        float height = transform.position.y;
        alphaCalculatedBasedOnHeightOfPlayer = Mathf.Clamp((fadeHeight - height) / fadeThickness, 0, 1);

        //snow
        if (particleSystem != null)
        {
            var emissionModule = particleSystem.emission;
            emissionModule.rateOverTime = (1-alphaCalculatedBasedOnHeightOfPlayer) * amountOfParticles;
        }


        //vignette
        if (vignetteRenderer != null)
        {
            vignetteRenderer.color = new Color(1, 1, 1, alphaCalculatedBasedOnHeightOfPlayer);
        }

        //snowstorm
        UpdateSnowstormVolume();
    }

    private void UpdateSnowstormVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = (1 - alphaCalculatedBasedOnHeightOfPlayer) * audioSourceVolumeMultiplierThroughHut;
        }
    }

    private void OnHutStateChange(bool isOpen)
    {
        audioSourceVolumeMultiplierThroughHut = (isOpen ? 0 : 1);
        UpdateSnowstormVolume();
    }
}
