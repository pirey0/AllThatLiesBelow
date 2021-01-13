using System;
using System.Collections;
using UnityEngine;

public class Radio : MineableObject, IBaseInteractable
{
    [SerializeField] AudioSource switchingSource, channelSource;
    [SerializeField] AudioClip[] switches, channelClips;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float maxVolume;

    [Zenject.Inject] CameraController cameraController;

    bool inTransition;
    public void BeginInteracting(GameObject interactor)
    {
        if (!inTransition)
        {
            StopAllCoroutines();
            StartCoroutine(TransitionCoroutine(switches[UnityEngine.Random.Range(0,switches.Length)], channelClips[UnityEngine.Random.Range(0, channelClips.Length)]));
            spriteRenderer.sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];
            cameraController.Shake(transform.position, shakeType: CameraShakeType.explosion, 0.25f, 10, 0.25f);
        }
    }

    IEnumerator TransitionCoroutine(AudioClip transition, AudioClip target)
    {
        inTransition = true;
        float length = transition.length;
        float t = 0;

        switchingSource.clip = transition;
        switchingSource.pitch = UnityEngine.Random.Range(0.75f, 1.5f);
        switchingSource.Play();

        while ((t+= Time.deltaTime) < (length / 2))
        {
            channelSource.volume = maxVolume * (1 - (t / (length / 2)));
            yield return null;
        }

        channelSource.clip = target;
        if (!channelSource.isPlaying) channelSource.Play();

        while ((t += Time.deltaTime) < (length))
        {
            channelSource.volume = maxVolume * (((t - (length / 2)) / (length / 2)));
            yield return null;
        }

        inTransition = false;
    }
}
