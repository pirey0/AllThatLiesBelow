using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
    [NaughtyAttributes.OnValueChanged("OnTargetChanged")]
    [SerializeField] int Framerate;


    private void Awake()
    {
        if (Framerate > 0)
            OnTargetChanged();
    }

    private  void OnTargetChanged()
    {
        if (!Application.isPlaying)
            return;

        Application.targetFrameRate = Framerate;
        QualitySettings.vSyncCount = 0;
        Debug.Log("New Target Framerate: " + Application.targetFrameRate );
    }

}