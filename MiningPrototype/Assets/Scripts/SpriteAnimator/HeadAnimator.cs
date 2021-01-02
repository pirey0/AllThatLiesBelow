using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class HeadAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] sprites, head, helmet, helmetWithLamp;
    
    [SerializeField] bool flipX;
    [SerializeField] SpriteRenderer playerSpriteRenderer;

    [Range(0f,10f)]
    [SerializeField] float mouseFollowMultiplier = 10f;

    [Range(0f,25f)]
    [SerializeField] float correctionultiplier = 10f;
    float angleBefore = 0;

    [SerializeField] bool hasHelmet;
    [SerializeField] bool hasLamp;

    [Zenject.Inject] CameraController cameraController;

    public void ChangeHelmetState(HelmentState newHelmentState)
    {
        sprites = GetArrayForState(newHelmentState);
    }

    private void Update()
    {
        flipX = TryFlipX();
        SetFrame(GetFrameFromMouseAngle());
    }

    private void SetFrame(int frameToSet)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprites[frameToSet];
    }

    private Sprite[] GetArrayForState(HelmentState state)
    {
        switch(state)
        {
            case HelmentState.Helmet:
                return helmet;
            case HelmentState.HelmetWidthLamp:
                return helmetWithLamp;
        }

        return head;
    }

    private bool TryFlipX()
    {
        bool xIsFlipped = playerSpriteRenderer == null ? false : playerSpriteRenderer.flipX;
        spriteRenderer.flipX = xIsFlipped;
        return xIsFlipped;
    }

    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }

    private void OnDisable()
    {
        spriteRenderer.enabled = false;
    }

    private int GetFrameFromMouseAngle()
    {
        Vector2 head = transform.position;
        Vector2 target = (cameraController == null) ? Vector3.right : Util.MouseToWorld(cameraController.Camera);

        float angle = Mathf.Atan2((target.x - head.x) / 2, target.y - head.y) * Mathf.Rad2Deg; //range between -180 and 180 (top is 0)
        float angleGeneralized = ((Mathf.Abs(angle) - 90) / 90f); //maps it onto -1 to 1 scale

        float targetIsNotZero = 1f - Mathf.Abs(angleGeneralized) * 0.9f + (Mathf.Sign(angleBefore) != Mathf.Sign(angleGeneralized)?1:0); //multiplier that is greater the more close the target is to looking forward or in the other direction

        float angleTarget = Mathf.MoveTowards(angleBefore,angleGeneralized,Time.deltaTime * mouseFollowMultiplier * (targetIsNotZero * correctionultiplier)); //moves to the target angle based on the multiplier
        float angleAdapted = Mathf.Clamp(Mathf.Pow(angleTarget * 2.3f,2) * Mathf.Sign(angleTarget), -5,5); //adapt the angle to the correct frames
        angleBefore = angleTarget;

        return Mathf.RoundToInt(5 + Mathf.RoundToInt(angleAdapted));
    }
}
