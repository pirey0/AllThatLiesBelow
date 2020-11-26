using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingForSpriteRenderer : MonoBehaviour
{
    [SerializeField] Color blinkColor;
    [SerializeField] Material blinkingMaterial;

    Material matBefore;
    Color colorBefore;

    SpriteRenderer spriteRenderer;
    Coroutine blinkingRoutine;
    bool isBlinking = false;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [Button]
    public void StartBlinking()
    {
        if (blinkingRoutine == null && spriteRenderer != null)
        {
            Debug.LogWarning("Start Blinking");
            matBefore = spriteRenderer.material;
            colorBefore = spriteRenderer.color;
            spriteRenderer.material = blinkingMaterial;
            blinkingRoutine = StartCoroutine(BlinkingRoutine());
        }
    }

    IEnumerator BlinkingRoutine()
    {
        float blinkSpeed = 8;

        isBlinking = true;
        float t = 0;

        while(isBlinking)
        {
            yield return null;
            spriteRenderer.color = blinkColor * (((Mathf.Sin(t * blinkSpeed) + 1) / 2));

            Debug.LogWarning("Update Blinking");

            t += Time.deltaTime;
        }
    }

    [Button]
    public void StopBlinking()
    {
        isBlinking = false;
        blinkingRoutine = null;

        spriteRenderer.material = matBefore;
        spriteRenderer.color = colorBefore;
    }
}
