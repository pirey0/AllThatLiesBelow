using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int idleFrame;
    [SerializeField] PickaxeFrames[] sprites;
    [SerializeField] AnimationCurve offsetOnSwing;
    [SerializeField] int offsetToMouse = 2;
    [SerializeField] bool hideSwing;

    int pickaxeLevel = 1;

    [Zenject.Inject] CameraController cameraController;
    private void Start()
    {
        SetFrame(idleFrame);
    }

    public void SetPickaxeLevel(int newLevel)
    {
        pickaxeLevel = newLevel;
        SetFrame(idleFrame);
        Debug.Log("Upgrade Pickaxe level: " + newLevel);
    }
    public void Play()
    {
        gameObject.SetActive(true);
        StartCoroutine(PickingRoutine());
    }

    public void Stop()
    {
        SetFrame(idleFrame);
        StopAllCoroutines();
    }

    IEnumerator PickingRoutine()
    {
        float currentTime = 0;
        int frameBefore = idleFrame;

        while (true)
        {
            //Debug.Log("play curve");
            currentTime += Time.deltaTime;
            int frameCurrent = GetFrameFromMouseAngle(!hideSwing?Mathf.RoundToInt(offsetOnSwing.Evaluate(currentTime)):0f);

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
            int framesMax = sprites[pickaxeLevel-1].sprites.Length;

            while (frameToSet < 12)
                frameToSet += framesMax;

            int frameRemapped = (frameToSet) % framesMax;

            //Debug.Log("frame:" + frameRemapped);
            spriteRenderer.sprite = sprites[pickaxeLevel-1].sprites[frameRemapped];
        }
    }

    private int GetFrameFromMouseAngle(float additionalAngle = 0)
    {
        Vector2 head = transform.position;
        Vector2 target = Util.MouseToWorld(cameraController.Camera);

        float angle = Mathf.Atan2((target.x - head.x) / 2, target.y - head.y) * Mathf.Rad2Deg; //range between -180 and 180 (top is 0)
        float angleGeneralized = ((((Mathf.Abs(angle) - 90) / 90f) + 1) * 2) - offsetToMouse; //maps it onto 0 to 4 for the full range on one side

        return Mathf.RoundToInt(angleGeneralized + additionalAngle);
    }

    public void Upgrade()
    {
        throw new System.NotImplementedException("Please use the PlayerVisualController for such Upgrades and remove this function, thank you. Have a nice day.");
    }
}

[System.Serializable]
public class PickaxeFrames
{
    public Sprite[] sprites;
}
