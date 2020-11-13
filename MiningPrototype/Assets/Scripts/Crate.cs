using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, IMinableNonGrid
{
    [SerializeField] SpriteRenderer spriteRenderer;
    public void Damage(float v)
    {
        spriteRenderer.color = Color.red;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void MouseEnter()
    {
        spriteRenderer.color = Color.yellow;
        Debug.LogWarning("enter");
    }

    public void MouseLeave()
    {
        spriteRenderer.color = Color.white;
        Debug.LogWarning("leave");
    }
}
