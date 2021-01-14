using System;
using System.Collections;
using UnityEngine;

public class Radio : MineableObject, IBaseInteractable, INonPersistantSavable
{
    [SerializeField] AudioSource switchingSource, channelSource;
    [SerializeField] AudioClip[] switches, channelClips;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float maxVolume;

    [Zenject.Inject] CameraController cameraController;

    bool inTransition;
    int radioIndex = 0;

    public void BeginInteracting(GameObject interactor)
    {
        if (!inTransition)
        {
            StopAllCoroutines();
            radioIndex = (radioIndex + 1) % channelClips.Length;
            TryPlayIndex(radioIndex);

            var spriteIndex = radioIndex % sprites.Length;
            spriteRenderer.sprite = sprites[spriteIndex];
            cameraController.Shake(transform.position, shakeType: CameraShakeType.explosion, 0.25f, 10, 0.25f);
        }
    }

    private void TryPlayIndex(int index)
    {
        var clip = channelClips[index];
        var transition = switches[UnityEngine.Random.Range(0, switches.Length)];
        if (clip == null)
            TurnOff(transition);
        else
            StartCoroutine(TransitionCoroutine(transition, channelClips[index]));
    }

    private void TurnOff(AudioClip transition)
    {
        switchingSource.clip = transition;
        switchingSource.pitch = UnityEngine.Random.Range(0.75f, 1.5f);
        switchingSource.Play();
        channelSource.Stop();
    }

    IEnumerator TransitionCoroutine(AudioClip transition, AudioClip target)
    {
        inTransition = true;
        float length = transition.length;
        float t = 0;

        switchingSource.clip = transition;
        switchingSource.pitch = UnityEngine.Random.Range(0.75f, 1.5f);
        switchingSource.Play();

        while ((t += Time.deltaTime) < (length / 2))
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

    public SpawnableSaveData ToSaveData()
    {
        var data = new RadioSaveData();
        data.SaveTransform(transform);
        data.Channel = radioIndex;
        return data;
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID("Radio");
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is RadioSaveData data)
        {
            radioIndex = data.Channel;
            TryPlayIndex(radioIndex);
        }
    }

    [System.Serializable]
    private class RadioSaveData : SpawnableSaveData
    {
        public int Channel;
    }
}
