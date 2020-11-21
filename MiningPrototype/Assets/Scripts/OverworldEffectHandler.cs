using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEffectHandler : StateListenerBehaviour
{
    [SerializeField] float fadeHeight;
    [SerializeField] float fadeThickness;

    [SerializeField] SpriteRenderer vignetteRenderer;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] float amountOfParticles;
    [SerializeField] AudioSource snowstormSounds, caveSounds;
    [SerializeField] float maxSnowStormVolume, maxCaveVolume;

    float alphaCalculatedBasedOnHeightOfPlayer;
    float audioSourceVolumeMultiplierThroughHut = 1;
    Hut hut;



    protected override void OnStateChanged(GameState.State newState)
    {
        if (newState == GameState.State.Playing)
        {
            hut = FindObjectOfType<Hut>();
            if (hut != null)
                hut.OnHutStateChange += OnHutStateChange;

            UpdateOverworldEffects();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (hut != null)
            hut.OnHutStateChange -= OnHutStateChange;
    }

    private void FixedUpdate()
    {
        if (transform.position.y < fadeHeight - fadeThickness || transform.position.y > fadeHeight + fadeThickness)
            return;

        UpdateOverworldEffects();
    }

    private void UpdateOverworldEffects()
    {
        float height = transform.position.y;
        alphaCalculatedBasedOnHeightOfPlayer = Mathf.Clamp((fadeHeight - height) / fadeThickness, 0, 1);

        //snow
        if (particleSystem != null)
        {
            var emissionModule = particleSystem.emission;
            emissionModule.rateOverTime = (1 - alphaCalculatedBasedOnHeightOfPlayer) * amountOfParticles;
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
        if (snowstormSounds != null)
            snowstormSounds.volume = (1 - alphaCalculatedBasedOnHeightOfPlayer) * audioSourceVolumeMultiplierThroughHut * maxSnowStormVolume;

        if (caveSounds != null)
            caveSounds.volume = alphaCalculatedBasedOnHeightOfPlayer * maxCaveVolume;
    }

    private void OnHutStateChange(bool isOpen)
    {
        audioSourceVolumeMultiplierThroughHut = (isOpen ? 0 : 1);
        UpdateSnowstormVolume();
    }
}
