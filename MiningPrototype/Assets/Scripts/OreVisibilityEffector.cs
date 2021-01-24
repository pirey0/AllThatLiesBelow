using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreVisibilityEffector : MonoBehaviour
{
    [SerializeField] float timeLeft = 60;
    [Zenject.Inject] RuntimeProceduralMap runtimeProceduralMap;

    void Start()
    {
        StartCoroutine(SetOreVisibleForCoroutine(runtimeProceduralMap, timeLeft, Callback));
    }

    private void Callback()
    {
        Destroy(gameObject);
    }

    public static IEnumerator SetOreVisibleForCoroutine(RuntimeProceduralMap map, float time, System.Action callback = null)
    {
        map.SetOresAllwaysVisible(true);

        yield return new WaitForSeconds(time);
        map.SetOresAllwaysVisible(false);
        callback?.Invoke();
    }

    
}

