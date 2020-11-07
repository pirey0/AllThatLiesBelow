using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionBasedAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int idleFrame = 0;
    [SerializeField] Sprite[] sprites;
    [SerializeField] AnimationCurve animationCurve;
    
    bool flipX;
    [SerializeField] SpriteRenderer playerSpriteRenderer;

    private void Update()
    {
        //SetFrame(GetFrameFromMouseAngle());
    }
    private void Start()
    {
        SetFrame(idleFrame);
    }
    public void Play()
    {
        StartCoroutine(PickingRoutine());
    }

    public void Stop()
    {
        StopAllCoroutines();
        SetFrame(idleFrame);
    }

    IEnumerator PickingRoutine()
    {
        float currentTime = 0;
        int frameBefore = GetFrameFromMouseAngle();
        flipX = playerSpriteRenderer == null?false:playerSpriteRenderer.flipX;

        while (true)
        {
            Debug.Log("play curve");
            currentTime += Time.deltaTime;
            int frameCurrent = GetFrameFromMouseAngle() + (int)animationCurve.Evaluate(currentTime) * (flipX?-1:1);

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
        return (int)((angle - 45f) / -45f);
    }
}
