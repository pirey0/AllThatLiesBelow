using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : BasicNonPersistantSavable
{
    [SerializeField] AudioSource bounceAudio;
    [SerializeField] ParticleSystem particleSystem;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bounceAudio.isPlaying)
            return;

        bounceAudio.pitch = UnityEngine.Random.Range(0.6f, 1);
        bounceAudio.Play();
        particleSystem.Play();
    }
}
