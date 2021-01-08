using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] AudioSource bounceAudio;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation[] spriteAnimation;

    [Zenject.Inject] CameraController cameraController;

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
        cameraController.Shake(transform.position, CameraShakeType.explosion, 0.15f, 10, 0.25f);
        bounceAudio.Play();
        particleSystem.Play();
        TryPlayAnimation();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryPlayAnimation();
    }
}
