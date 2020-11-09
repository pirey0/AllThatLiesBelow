using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEffectHandler : MonoBehaviour
{
    [SerializeField] float fadeHeight;
    [SerializeField] float fadeThickness;

    [SerializeField] SpriteRenderer vignetteRenderer;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] float amountOfParticles;

    [SerializeField] AudioSource audioSource;
    
    private void FixedUpdate()
    {
        if (transform.position.y < fadeHeight - fadeThickness || transform.position.y > fadeHeight + fadeThickness)
            return;

        float height = transform.position.y;
        float alpha = Mathf.Clamp((fadeHeight - height) / fadeThickness, 0, 1);

        //snow
        if (particleSystem != null)
        {
            var emissionModule = particleSystem.emission;
            emissionModule.rateOverTime = (1-alpha) * amountOfParticles;
        }


        //vignette
        if (vignetteRenderer != null)
        {
            vignetteRenderer.color = new Color(1, 1, 1, alpha);
        }

        //snowstorm
        if (audioSource != null)
        {
            audioSource.volume = (1 - alpha);
        }
    }
}
