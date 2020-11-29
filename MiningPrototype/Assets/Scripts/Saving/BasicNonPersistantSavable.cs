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

        //Wrap position in case Mirror Follower is in mirrored position
        var pos = transform.position;
        int sizeX = RuntimeProceduralMap.Instance.SizeX;
        pos.x = (pos.x+sizeX) % sizeX;

        data.Position = new SerializedVector3(pos);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        return data;
    }

   
}
