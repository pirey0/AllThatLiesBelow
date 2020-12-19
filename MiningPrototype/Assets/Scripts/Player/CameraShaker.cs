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

    List<IShake> shakes = new List<IShake>();
    [SerializeField] bool showShakes;

    private void Start()
    {
        StartCoroutine(CameraShakeRoutine());
    }

    //defaultShakeTypes
    public CameraShake StartShake(CameraShakeType type, float duration, Vector2 location, float range, float intensity)
    {
        CameraShake cs = new CameraShake(GetCurveForType(type), location, duration, range, intensity);
        shakes.Add(cs);
        return cs;
    }

    //customShakeFromCurve
    public CameraShake StartShake(AnimationCurve customCurve, float duration, Vector2 location, float range, float intensity)
    {
        CameraShake cs = new CameraShake(customCurve, location, duration, range, intensity);
        shakes.Add(cs);
        return cs;
    }

    public ParentedCameraShake StartParentedShake(Transform parent, float range, float intensity)
    {
        ParentedCameraShake cameraShake = new ParentedCameraShake(parent, range, intensity);
        shakes.Add(cameraShake);
        return cameraShake;
    }

    public void StopShake(IShake shake)
    {
        if (shakes.Contains(shake))
            shakes.Remove(shake);
    }

    private IEnumerator CameraShakeRoutine()
    {
        while (true)
        {
            float intensity = 0;

            for (int i = shakes.Count - 1; i >= 0; i--)
            {
                float shakeIntensity = shakes[i].GetIntensity(transform.position);

                if (shakeIntensity < 0)
                {
                    shakes.RemoveAt(i);
                }
                else if (shakeIntensity > intensity)
                {
                    intensity = shakeIntensity;
                }
            }

            if (intensity > 0)
            {
                intensity *= 10 * Time.deltaTime;
                shakeAmount = Vector2.MoveTowards(Vector2.zero, new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)), intensity);
            }
            else
            {
                shakeAmount = Vector2.zero;// Vector2.MoveTowards(shakeAmount, new Vector2(0,0), Time.deltaTime);
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
            shake.DrawGizmos();
        }
    }
}

public interface IShake
{
    float GetIntensity(Vector2 cameraLocation);
    void DrawGizmos();
}


public class CameraShake : IShake
{
    float timeAtStart;
    float timeAtEnd;
    float curveLength;
    AnimationCurve overTime;
    float duration;
    Vector2 location;
    float rangeForFalloff = 10;
    float intensity;

    public CameraShake(AnimationCurve curve, Vector2 _location, float _duration, float _range, float _intensity)
    {
        timeAtStart = Time.time;
        curveLength = curve.keys[curve.length - 1].time;
        timeAtEnd = timeAtStart + _duration;
        duration = _duration;

        overTime = curve;

        location = _location;
        rangeForFalloff = _range;
        intensity = _intensity;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawWireSphere(location, rangeForFalloff);
    }

    public float GetIntensity(Vector2 cameraLocation)
    {
        //time is up, please kill me
        if (Time.time > timeAtEnd)
        {
            return -1;
        }

        return intensity * overTime.Evaluate(((Time.time - timeAtStart) / curveLength) / duration) * (1 - Mathf.Clamp(Vector2.Distance(cameraLocation, location) / rangeForFalloff, 0, 1));
    }
}

public class ParentedCameraShake : IShake
{
    float rangeForFalloff = 10;
    float intensity;
    Transform parent;
    bool done = false;
    public ParentedCameraShake(Transform parent, float _range, float _intensity)
    {
        rangeForFalloff = _range;
        intensity = _intensity;
        this.parent = parent;
    }

    public void DrawGizmos()
    {
        Gizmos.DrawWireSphere(parent.position, rangeForFalloff);
    }

    public void Stop()
    {
        done = true;
    }

    public void SetIntensity(float i)
    {
        intensity = i;
    }

    public float GetIntensity(Vector2 cameraLocation)
    {
        if (done)
        {
            return -1;
        }

        return intensity * (1 - Mathf.Clamp(Vector2.Distance(cameraLocation, parent.position) / rangeForFalloff, 0, 1));
    }
}