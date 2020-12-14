using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Daylight : StateListenerBehaviour, INonPersistantSavable
{
    [SerializeField] Light2D light;
    [SerializeField] float[] modeIntensities;
    [SerializeField] [NaughtyAttributes.ReadOnly] Lightmode currentMode;

    public enum Lightmode { Day, Night}

    protected override void OnRealStart()
    {
        SetIntensity(currentMode);
    }
    public void SetIntensity (Lightmode mode)
    {
        currentMode = mode;
        //light.intensity = modeIntensities[(int)mode];
        light.enabled = (mode == Lightmode.Day);
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new DaylightSaveData();
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.SpawnableIDType = SpawnableIDType.OverworldLight;
        data.Lightmode = currentMode;

        return data;
    }

    public void Load(SpawnableSaveData data)
    {
        if(data is DaylightSaveData sdata)
        {
            currentMode = sdata.Lightmode;
        }
    }

    [System.Serializable]
    public class DaylightSaveData : SpawnableSaveData
    {
        public Lightmode Lightmode;
    }
}
