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
    [SerializeField] ParticleSystem snow, snowSecondary, clouds;
    [SerializeField] float amountOfParticles;
    [SerializeField] AudioSource snowstormSounds, caveSounds, springSounds;
    [SerializeField] float maxSnowStormVolume, maxCaveVolume;
    [SerializeField] bool isNight;

    [SerializeField] bool isSpring = false;

    [Zenject.Inject] CameraController cam;
    [Zenject.Inject] PlayerStateMachine player;
    [Zenject.Inject] RuntimeProceduralMap map;

    float snowMultiplyer = 1;
    float alphaCalculatedBasedOnHeightOfPlayer;
    float audioSourceVolumeMultiplierThroughHut = 1;
    Hut hut;
    Daylight daylight;
    RuntimeProceduralMap.MirrorState mirrorState;

    protected override void OnRealStart()
    {
        hut = FindObjectOfType<Hut>();
        if (hut != null)
            hut.OnHutStateChange += OnHutStateChange;

        daylight = FindObjectOfType<Daylight>();

        transform.position = cam.transform.position + offset;
        caveSounds.Play();
        snowstormSounds.Play();
        springSounds.Play();
        UpdateOverworldEffects();
        map.MirrorSideChanged += OnMirrorSideChanged;
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        map.MirrorSideChanged -= OnMirrorSideChanged;

        if (hut != null)
            hut.OnHutStateChange -= OnHutStateChange;
    }

    public void OnMirrorSideChanged(RuntimeProceduralMap.MirrorState newState)
    {
        mirrorState = newState;
        if (mirrorState == RuntimeProceduralMap.MirrorState.Left)
            snowSecondary.transform.localPosition = new Vector3(200, 0, 0);
        else if (mirrorState == RuntimeProceduralMap.MirrorState.Right)
            snowSecondary.transform.localPosition = new Vector3(-200, 0, 0);

        UpdateOverworldEffects();
    }

    private void FixedUpdate()
    {
        transform.position = cam.transform.position + offset;

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
            float rate = (1 - alphaCalculatedBasedOnHeightOfPlayer) * amountOfParticles * snowMultiplyer * (isSpring ? 0f : 1f);
            var snowEmission = snow.emission;
            snowEmission.rateOverTime = rate;

            snowEmission = snowSecondary.emission;

            if (mirrorState == RuntimeProceduralMap.MirrorState.Center)
                snowEmission.rateOverTime = 0;
            else
                snowEmission.rateOverTime = rate;
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
        }
        else
        {
            if (springSounds != null)
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
