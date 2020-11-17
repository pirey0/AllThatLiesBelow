using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ISavable
{
    SaveData ToSaveData();

    void Load(SaveData data);

    int GetInstanceID();
}

[System.Serializable]
public class SaveData
{
    public int InstanceID;

    
}