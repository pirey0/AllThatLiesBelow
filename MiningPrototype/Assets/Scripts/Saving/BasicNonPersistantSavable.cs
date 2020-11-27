using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNonPersistantSavable : MonoBehaviour, INonPersistantSavable
{
    [SerializeField] SpawnableIDType type;
    public void Load(SpawnableSaveData data)
    {
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new SpawnableSaveData();
        data.SpawnableIDType = type;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        return data;
    }

   
}
