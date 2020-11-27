using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEffectHandler : StateListenerBehaviour
{
    [SerializeField] float fadeHeight;
    [SerializeField] float fadeThickness;

    [SerializeField] SpriteRenderer vignetteRenderer;
    [SerializeField] ParticleSystem snow, clouds;
    [SerializeField] float amountOfParticles;
    [SerializeField] AudioSource snowstormSounds, caveSounds, springSounds;
    [SerializeField] float maxSnowStormVolume, maxCaveVolume;

    [SerializeField] bool isSpring = false;

    float alphaCalculatedBasedOnHeightOfPlayer;
    float audioSourceVolumeMultiplierThroughHut = 1;
    Hut hut;


    protected override void OnStartAfterLoad()
    {
        hut = FindObjectOfType<Hut>();
        if (hut != null)
            hut.OnHutStateChange += OnHutStateChange;

        UpdateOverworldEffects();
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
        if (snow != null)
        {
            var snowEmission = snow.emission;
            snowEmission.rateOverTime = (1 - alphaCalculatedBasedOnHeightOfPlayer) * amountOfParticles * (isSpring?0f:1f);
        }

        //clouds
        if (clouds != null && isSpring)
        {
            var cloudEmission = clouds.emission;
            cloudEmission.rateOverTime = 0.1f;
        }


        //vignette
        if (vignetteRenderer != null)
        {
            vignetteRenderer.color = new Color(1, 1, 1, alphaCalculatedBasedOnHeightOfPlayer);
        }

        //sounds
        UpdateSounds();
    }

    private void UpdateSounds()
    {
        if (isSpring)
        {
            if (springSounds != null)
                springSounds.volume = (1 - alphaCalculatedBasedOnHeightOfPlayer) * audioSourceVolumeMultiplierThroughHut * maxSnowStormVolume;

            if (snowstormSounds != null)
                snowstormSounds.volume = 0;
        } else
        {
            if(springSounds != null)
                springSounds.volume = 0;

            if (snowstormSounds != null)
                snowstormSounds.volume = (1 - alphaCalculatedBasedOnHeightOfPlayer) * audioSourceVolumeMultiplierThroughHut * maxSnowStormVolume;
        }

        if (caveSounds != null)
            caveSounds.volume = alphaCalculatedBasedOnHeightOfPlayer * maxCaveVolume;
    }

    private void OnHutStateChange(bool isOpen)
    {
        audioSourceVolumeMultiplierThroughHut = (isOpen ? 0 : 1);
        UpdateSounds();
    }

    [Button]
    public void MakeSpring()
    {
        isSpring = true;
        UpdateOverworldEffects();
    }
}
