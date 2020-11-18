using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum CameraShakeType
{
    hill,
    explosion,
    raising,
}

public class CameraShaker : MonoBehaviour
{
    Vector2 shakeAmount = Vector2.zero;
    [SerializeField] AnimationCurve hill;
    [SerializeField] AnimationCurve explosion;
    [SerializeField] AnimationCurve raising;

    List<CameraShake> shakes = new List<CameraShake>();
    [SerializeField] bool showShakes;

    private void Start ()
    {
        StartCoroutine(CameraShakeRoutine());
    }

    //defaultShakeTypes
    public CameraShake StartShake(CameraShakeType type, float duration, Vector2 location, float range)
    {
        CameraShake cs = new CameraShake(GetCurveForType(type), location, duration, range);
        shakes.Add(cs);
        return cs;
    }

    //customShakeFromCurve
    public CameraShake StartShake(AnimationCurve customCurve, float duration, Vector2 location, float range)
    {
        CameraShake cs = new CameraShake(customCurve, location, duration, range);
        shakes.Add(cs);
        return cs;
    }

    internal void StopShake(CameraShake shake)
    {
        if (shakes.Contains(shake))
            shakes.Remove(shake);
    }

    private IEnumerator CameraShakeRoutine ()
    {
        while (true)
        {
            float intensity = 0;

            for (int i = shakes.Count - 1; i >= 0; i--)
            {
                float shakeIntensity = shakes[i].GetIntensity(transform.position);

                if (shakeIntensity < 0)
                    shakes.RemoveAt(i);
                else
                    intensity += shakeIntensity;
            }

            if (intensity > 0)
            {

                intensity *= 10 * Time.deltaTime;
                shakeAmount = Vector2.MoveTowards(shakeAmount, new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)), intensity);
            }
            yield return null;
        }
    }
    public Vector3 GetShakeAmount()
    {
        return shakeAmount;
    }

    public AnimationCurve GetCurveForType(CameraShakeType type)
    {
        switch (type)
        {
            case CameraShakeType.explosion:
                return explosion;

            case CameraShakeType.raising:
                return raising;
        }

        return hill;
    }

    private void OnDrawGizmos()
    {
        foreach (CameraShake shake in shakes)
        {
            Gizmos.DrawWireSphere(shake.location, shake.rangeForFalloff);
        }
    }
}

public class CameraShake
{
    float timeAtStart;
    float timeAtEnd;
    float curveLength;
    AnimationCurve overTime;
    float duration;
    public Vector2 location;
    public float rangeForFalloff = 10;

    public CameraShake (AnimationCurve curve, Vector2 _location, float _duration, float _range)
    {
        timeAtStart = Time.time;
        curveLength = curve.keys[curve.length - 1].time;
        timeAtEnd = timeAtStart + _duration;
        duration = _duration;

        overTime = curve;

        location = _location;
        rangeForFalloff = _range;
    }
    public float GetIntensity(Vector2 cameraLocation)
    {
        //time is up, please kill me
        if (Time.time > timeAtEnd)
        {
            return -1;
        }

        return overTime.Evaluate(((Time.time - timeAtStart)/curveLength) / duration) * (1 - Mathf.Clamp(Vector2.Distance(cameraLocation, location) / rangeForFalloff, 0, 1));
    }
}
