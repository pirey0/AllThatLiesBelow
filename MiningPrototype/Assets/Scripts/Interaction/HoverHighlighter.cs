using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoverable
{
    bool IsInteractable { get; }

    void HoverEnter(bool isDraggingItem);
    void HoverExit();
    void HoverUpdate();
}

[RequireComponent(typeof(SpriteRenderer))]
public class HoverHighlighter : MonoBehaviour, IHoverable
{
    [SerializeField] Shader shader;
    SpriteRenderer parentRenderer;
    SpriteRenderer spriteRenderer;

    bool isInteractable, isDropreceiver;

    float blinkDelay = 0.2f;
    Color blinkColor;
    Coroutine blinkRoutine;
    bool IsBlinking
    {
        set
        {
            if (value == true)
            {
                if (blinkRoutine == null)
                    blinkRoutine = StartCoroutine(BlinkRoutine());
            } else
            {
                if (blinkRoutine != null)
                {
                    StopCoroutine(blinkRoutine);
                    blinkRoutine = null;
                    Material.SetColor("_OverlayColor", new Color(0, 0, 0, 0));
                }
            }
        }
        get => (blinkRoutine != null);
    }

    Material material;
    Material Material
    {
        get
        {
            if (material == null)
                material = SetupMaterial();

            return material;
        }
    }

    public bool IsInteractable => isInteractable;

    private void OnEnable()
    {
        isInteractable = GetComponent<IBaseInteractable>() != null;
        isDropreceiver = GetComponent<IDropReceiver>() != null;

        parentRenderer = GetComponent<SpriteRenderer>();
    }

    private Material SetupMaterial()
    {
        if (shader != null)
        {
            GameObject child = new GameObject();
            child.transform.parent = transform;
            child.transform.localPosition = Vector3.zero;
            spriteRenderer = child.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = parentRenderer.sprite;
            spriteRenderer.sortingLayerID = SortingLayer.NameToID("Player");
            spriteRenderer.sortingOrder = -1000;
            spriteRenderer.material = new Material(shader);

            return spriteRenderer.material;
        }
        else
        {
            Debug.LogError("Please assign Sprit-Lit-Interactable shader to HoverHighlighter on " + name);
            Destroy(this);
        }
        return null;
    }

    IEnumerator BlinkRoutine()
    {
        bool on = true;
        while (true)
        {
            blinkColor = on ? Color.white : new Color(0, 0, 0, 0);
            on = !on;
            Material.SetColor("_OverlayColor", blinkColor);
            yield return new WaitForSeconds(blinkDelay);
        }
    }


    [Button]
    public void StartBlinking()
    {
        IsBlinking = true;
    }

    [Button]
    public void StopBlinking()
    {
        IsBlinking = false;
    }

    public void HoverEnter(bool isDraggingItem)
    {
        if (isInteractable || (isDropreceiver && isDraggingItem))
        {
            Material.color = Color.white;
            Material.SetColor("_OverlayColor", IsBlinking ? blinkColor : new Color(0, 0, 0, 0.2f));
        }
    }

    public void HoverExit()
    {
        if (this == null)
            return;

        Material.color = new Color(0,0,0,0);
        Material.SetColor("_OverlayColor", IsBlinking ? blinkColor : new Color(0, 0, 0, 0));
    }

    public void HoverUpdate()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = parentRenderer.sprite;
    }
}
