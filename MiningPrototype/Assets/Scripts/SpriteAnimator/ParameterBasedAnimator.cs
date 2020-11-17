using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterBasedAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] sprites;
    
    [SerializeField] bool flipX;
    [SerializeField] SpriteRenderer playerSpriteRenderer;

    private void Update()
    {
        flipX = TryFlipX();
        SetFrame(GetFrameFromMouseAngle());
    }

    void SetFrame(int frameToSet)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprites[frameToSet];
    }

    private bool TryFlipX()
    {
        bool xIsFlipped = playerSpriteRenderer == null ? false : playerSpriteRenderer.flipX;
        spriteRenderer.flipX = xIsFlipped;
        return xIsFlipped;
    }

    private int GetFrameFromMouseAngle()
    {
        Vector2 center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        float angle = (Mathf.Atan2(Input.mousePosition.y - center.y, (Input.mousePosition.x - center.x) * (flipX?-1:1))) * Mathf.Rad2Deg;
        int angleAdapted = Mathf.RoundToInt(Mathf.Clamp((angle / -90f) * 3,-5,5));
        Debug.Log(angleAdapted);
       return Mathf.RoundToInt(5 + angleAdapted);
    }
}
