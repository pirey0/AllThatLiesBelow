using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnAwakeFinder : StateListenerBehaviour
{

    protected override void OnRealStart()
    {
        Debug.LogWarning("Serching for PlayOnAwake");

        var gos = GameObject.FindObjectsOfType<AudioSource>();

        foreach (var item in gos)
        {
            if (item.playOnAwake)
            {
                Debug.LogWarning("PlayOnAwake on: " + item.name);
            }
        }
    }
}
