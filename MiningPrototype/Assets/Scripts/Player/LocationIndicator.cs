using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationIndicator : MonoBehaviour, INonPersistantSavable
{
    public IndicatorType Type;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Util.GizmosDrawTile(transform.position.ToGridPosition());
    }

    public static LocationIndicator Find(IndicatorType type)
    {
        return GameObject.FindObjectsOfType<LocationIndicator>().First((x) => x.Type == type);
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new LocationIndicatorSaveData();
        data.Type = Type;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.SpawnableIDType = SpawnableIDType.LocationIndicator;
        return data;
    }

    public void Load(SpawnableSaveData sdata)
    {
        if(sdata is LocationIndicatorSaveData data)
        {
            Type = data.Type;
        }
    }

    [System.Serializable]
    public class LocationIndicatorSaveData : SpawnableSaveData
    {
        public IndicatorType Type;
    }
}
public enum IndicatorType
{
    PlayerStart,
    OrderSpawn
}
