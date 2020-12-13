using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverworldEffectHandler : StateListenerBehaviour
{

    [SerializeField] Vector3 offset;
    [SerializeField] float fadeHeight;
    [SerializeField] float fadeThickness;

    [SerializeField] SpriteRenderer nightSky;
    [SerializeField] ParticleSystem snow, clouds;
    [SerializeField] float amountOfParticles;
    [SerializeField] AudioSource snowstormSounds, caveSounds, springSounds;
    [SerializeField] float maxSnowStormVolume, maxCaveVolume;
    [SerializeField] bool isNight;

    [SerializeField] bool isSpring = false;

    [Zenject.Inject] PlayerStateMachine player;

    float snowMultiplyer;
    float alphaCalculatedBasedOnHeightOfPlayer;
    float audioSourceVolumeMultiplierThroughHut = 1;
    Hut hut;
    Daylight daylight;

    protected override void OnRealStart()
    {
        hut = FindObjectOfType<Hut>();
        if (hut != null)
            hut.OnHutStateChange += OnHutStateChange;

        daylight = FindObjectOfType<Daylight>();

        transform.position = player.transform.position + offset;
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
        transform.position = player.transform.position + offset;

        if (transform.position.y < fadeHeight - fadeThickness || transform.position.y > fadeHeight + fadeThickness)
            return;

        UpdateOverworldEffects();
    }

    public void SetNight()
    {
        isNight = true;
        UpdateOverworldEffects();
    }

    public void SetDay()
    {
        isNight = false;
        UpdateOverworldEffects();
    }

    public void SetSnowAmount(float amount)
    {
        snowMultiplyer = amount;
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
            snowEmission.rateOverTime = (1 - alphaCalculatedBasedOnHeightOfPlayer) * amountOfParticles * snowMultiplyer * (isSpring?0f:1f);
        }

        //clouds
        if (clouds != null && isSpring)
        {
            var cloudEmission = clouds.emission;
            cloudEmission.rateOverTime = 0.1f;
        }

        //sounds
        UpdateSounds();

        //NightSky
        nightSky.gameObject.SetActive(isNight);
        daylight?.SetIntensity(isNight ? Daylight.Lightmode.Night : Daylight.Lightmode.Day);
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
