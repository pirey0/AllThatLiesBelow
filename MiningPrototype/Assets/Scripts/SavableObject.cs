using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ISavable
{
    SaveData ToSaveData();

    void Load(SaveData data);

    string GetSaveID();
}

[System.Serializable]
public class SaveData
{
    public string GUID;
    
}