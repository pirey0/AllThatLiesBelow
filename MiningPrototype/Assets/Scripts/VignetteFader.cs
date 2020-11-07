using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VignetteFader : MonoBehaviour
{
    [SerializeField] SpriteRenderer vignetteRenderer;

    [SerializeField] float fadeHeight;
    [SerializeField] float fadeThickness;
    private void FixedUpdate()
    {
        if (vignetteRenderer != null)
        {
            float height = transform.position.y;
            float alpha = Mathf.Clamp((fadeHeight - height) / fadeThickness, 0, 1);
            vignetteRenderer.color = new Color(1, 1, 1, alpha);
        }
    }
}
