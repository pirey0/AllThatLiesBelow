using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : MonoBehaviour
{
    [SerializeField] AudioSource bounceAudio;
    [SerializeField] new ParticleSystem particleSystem;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.position.y > transform.position.y)
        {
            if (bounceAudio.isPlaying || collision.attachedRigidbody == null)
                return;

            var vel = collision.attachedRigidbody.velocity;
            if (vel.y < 0)
            {
                bounceAudio.pitch = UnityEngine.Random.Range(0.6f, 1);
                cameraController.Shake(transform.position, CameraShakeType.explosion, 0.15f, 10, 0.25f);
                bounceAudio.Play();
                particleSystem.Play();
                TryPlayAnimation();
                collision.attachedRigidbody.velocity = new Vector2(vel.x, Mathf.Abs(vel.y));
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryPlayAnimation();
    }
}
