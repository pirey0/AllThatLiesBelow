using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraPanner : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] float highOffset, veryHighOffset;
    [SerializeField] float transitionSpeed;
    [SerializeField] RectTransform topImage, botImage;
    [SerializeField] float barOpeningSpeed;

    [Zenject.Inject] PlayerManager playerManager;

    float yOffset;
    bool cinematicMode;

    float barhalfHeight;

    private void Start()
    {
        barhalfHeight = Screen.height / 12;
        topImage.sizeDelta = new Vector2(0, barhalfHeight * 2);
        botImage.sizeDelta = new Vector2(0, barhalfHeight * 2);
        botImage.anchoredPosition = new Vector2(0, -barhalfHeight);
        topImage.anchoredPosition = new Vector2(0, barhalfHeight);
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    [NaughtyAttributes.Button]
    public void EnterCinematicMode()
    {
        cinematicMode = true;
        StopAllCoroutines();
        StartCoroutine(TransitionCutscenebars(open: true));
    }

    [NaughtyAttributes.Button]
    public void ExitCinematicMode()
    {
        cinematicMode = false;
        StopAllCoroutines();
        StartCoroutine(TransitionCutscenebars(open: false));
    }

    public IEnumerator TransitionCutscenebars(bool open)
    {
        float currentY = topImage.anchoredPosition.y;
        float dir = open ? -1 : 1;

        while ((open) ? topImage.anchoredPosition.y > -barhalfHeight : topImage.anchoredPosition.y < barhalfHeight)
        {
            yield return null;
            topImage.anchoredPosition += new Vector2(0, dir * Time.deltaTime * barOpeningSpeed);
            botImage.anchoredPosition += new Vector2(0, -dir * Time.deltaTime * barOpeningSpeed);
        }
        botImage.anchoredPosition = new Vector2(0, -dir * barhalfHeight);
        topImage.anchoredPosition = new Vector2(0, dir * barhalfHeight);
    }

    public void UpdatePosition()
    {
        if (Time.timeScale == 0)
            return;

        Vector3 dir = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        dir = dir - new Vector3(0.5f, 0.5f);
        dir = new Vector3(dir.x.Sign() * curve.Evaluate(dir.x.Abs()), dir.y.Sign() * curve.Evaluate(dir.y.Abs()));

        var state = GetCurrentTargetState();
        var targetOffset = GetTargetOffset(state);

        if (yOffset > targetOffset)
        {
            yOffset -= transitionSpeed * Time.deltaTime;
            if (yOffset < targetOffset)
                yOffset = targetOffset;
        }
        else if (yOffset < targetOffset)
        {
            yOffset += transitionSpeed * Time.deltaTime;
            if (yOffset > targetOffset)
                yOffset = targetOffset;
        }

        transform.position = playerManager.GetPlayerPosition() + (cinematicMode ? Vector3.zero : dir) + new Vector3(0, yOffset);
    }

    private float GetTargetOffset(State state)
    {
        switch (state)
        {
            case State.VeryHigh:
                return veryHighOffset;

            case State.High:
                return highOffset;

        }

        return 0;
    }

    private State GetCurrentTargetState()
    {
        if (playerManager.GetPlayerInteraction().InDialog())
        {
            return State.VeryHigh;
        }
        else if (playerManager.GetPlayer().InOverworld() || cinematicMode)
        {
            return State.High;
        }

        return State.Normal;
    }

    public enum State
    {
        Normal,
        High,
        VeryHigh
    }
}
