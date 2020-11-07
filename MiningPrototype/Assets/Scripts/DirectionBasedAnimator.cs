using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionBasedAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int idleFrameRight,idleFrameLeft;
    [SerializeField] Sprite[] sprites;
    [SerializeField] AnimationCurve animationCurve;
    
    bool flipX;
    [SerializeField] SpriteRenderer playerSpriteRenderer;

    //private void Update()
    //{
    //    flipX = playerSpriteRenderer == null ? false : playerSpriteRenderer.flipX;
    //    SetFrame(GetFrameFromMouseAngle());
    //}
    private void Start()
    {
        SetFrame(idleFrameRight);
    }
    public void Play()
    {
        StartCoroutine(PickingRoutine());
    }

    public void Stop()
    {
        StopAllCoroutines();
        SetFrame(flipX?idleFrameLeft:idleFrameRight);
    }

    IEnumerator PickingRoutine()
    {
        float currentTime = 0;
        int frameBefore = GetFrameFromMouseAngle();
        flipX = playerSpriteRenderer == null?false:playerSpriteRenderer.flipX;
        spriteRenderer.flipX = flipX;

        while (true)
        {
            Debug.Log("play curve");
            currentTime += Time.deltaTime;
            int frameCurrent = GetFrameFromMouseAngle() + (int)animationCurve.Evaluate(currentTime);

            if (frameCurrent != frameBefore)
            {
                frameBefore = frameCurrent;
                SetFrame(frameCurrent);
            }
            yield return null;
        }
    }

    void SetFrame(int frameToSet)
    {
        if (spriteRenderer != null)
        {
            int framesMax = sprites.Length;

            while (frameToSet < 12)
                frameToSet += framesMax;

            int frameRemapped = (frameToSet) % framesMax;

            Debug.Log("frame:" + frameRemapped);
            spriteRenderer.sprite = sprites[frameRemapped];
        }
    }

    private int GetFrameFromMouseAngle()
    {
        Vector2 p2 = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        float angle = (Mathf.Atan2(Input.mousePosition.y - p2.y, Input.mousePosition.x - p2.x)) * Mathf.Rad2Deg;
        float angleAsFloat = (angle - 45f) / -45f;
        return Mathf.RoundToInt(angleAsFloat);
    }
}
