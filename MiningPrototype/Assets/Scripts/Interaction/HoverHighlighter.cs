using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoverable
{
    void HoverEnter();
    void HoverExit();
    void HoverUpdate();
}

public class HoverHighlighter : MonoBehaviour, IHoverable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Material spriteDefault, spriteOuline;

    public void HoverEnter()
    {
        spriteDefault = spriteRenderer.material;
        spriteRenderer.material = spriteOuline;
        spriteRenderer.color = new Color(0.8f,0.8f,0.8f);
    }

    public void HoverExit()
    {
        if (this == null)
            return;

        spriteRenderer.material = spriteDefault;
        spriteRenderer.color = Color.white;
    }

    public void HoverUpdate()
    {
        //
    }
}
