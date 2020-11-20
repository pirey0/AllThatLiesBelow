using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int idleFrameRight,idleFrameLeft;
    [SerializeField] Sprite[] sprites;
    [SerializeField] AnimationCurve animationCurve;


    private void Start()
    {
        SetFrame(idleFrameRight);
    }
    public void Play()
    {
        gameObject.SetActive(true);
        StartCoroutine(PickingRoutine());
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    IEnumerator PickingRoutine()
    {
        float currentTime = 0;
        int frameBefore = GetFrameFromMouseAngle();

        while (true)
        {
            //Debug.Log("play curve");
            currentTime += Time.deltaTime;
            int frameCurrent = GetFrameFromMouseAngle(Mathf.RoundToInt(animationCurve.Evaluate(currentTime) * 45f));

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

            //Debug.Log("frame:" + frameRemapped);
            spriteRenderer.sprite = sprites[frameRemapped];
        }
    }

    private int GetFrameFromMouseAngle(float additionalAngle = 0)
    {
        Vector2 p2 = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        float angle = (Mathf.Atan2(Input.mousePosition.y - p2.y, Input.mousePosition.x - p2.x)) * Mathf.Rad2Deg + additionalAngle * (-1);
        float angleAsFloat = (angle + (45)) / (45f);
        return Mathf.RoundToInt(angleAsFloat);
    }
}
