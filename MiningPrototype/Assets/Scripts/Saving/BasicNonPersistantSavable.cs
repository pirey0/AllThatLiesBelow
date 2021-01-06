using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNonPersistantSavable : MonoBehaviour, INonPersistantSavable
{

    [SerializeField] SpawnableIDType type;
    [Header("For new Objects: Set type to None and assign a uniqueName")]
    [SerializeField] string uniqueName;

    public SaveID GetSavaDataID()
    {
        if (type == SpawnableIDType.None)
            return new SaveID(uniqueName);
        else
            return new SaveID(type);
    }

    public void Load(SpawnableSaveData data)
    {
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new SpawnableSaveData();

        //Wrap position in case Mirror Follower is in mirrored position
        var pos = transform.position;
        if (RuntimeProceduralMap.Instance != null)
        {
            int sizeX = RuntimeProceduralMap.Instance.SizeX;
            pos.x = (pos.x + sizeX) % sizeX;
        }

        data.Position = new SerializedVector3(pos);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        return data;
    }


}
