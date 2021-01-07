using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : BasicNonPersistantSavable
{
    [SerializeField] AudioSource bounceAudio;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation[] spriteAnimation;

    int variant = 0;

    private void OnEnable()
    {
        variant = UnityEngine.Random.Range(0, spriteAnimation.Length);
        TryPlayAnimation();
    }

    private void TryPlayAnimation()
    {
        if (spriteAnimator != null && spriteAnimation.Length > variant)
        {
            spriteAnimator.Play(spriteAnimation[variant]);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bounceAudio.isPlaying)
            return;

        bounceAudio.pitch = UnityEngine.Random.Range(0.6f, 1);
        bounceAudio.Play();
        particleSystem.Play();        
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryPlayAnimation();
    }
}
