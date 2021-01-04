using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreVisibilityEffector : MonoBehaviour
{
    [SerializeField] float timeLeft = 60;
    [Zenject.Inject] RuntimeProceduralMap runtimeProceduralMap;

    bool active = false;
    void Start()
    {
        active = true;
        if (runtimeProceduralMap == null)
            runtimeProceduralMap = FindObjectOfType<RuntimeProceduralMap>();

        runtimeProceduralMap.SetOresAllwaysVisible(true);
    }

    private void End()
    {
        if (!active)
            return;

        if (runtimeProceduralMap == null)
            runtimeProceduralMap = FindObjectOfType<RuntimeProceduralMap>();

        runtimeProceduralMap.SetOresAllwaysVisible(false);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
            End();
    }
}
    
