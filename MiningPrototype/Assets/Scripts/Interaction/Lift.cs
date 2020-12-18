using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : TilemapCarvingEntity, INonPersistantSavable
{
    [SerializeField] LiftCage cage;

    private void Start()
    {
        Carve();
    }

    public void Load(SpawnableSaveData data)
    {
        if(data is LiftSaveData sdata)
        {
            cage.Load(sdata);
        }
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new LiftSaveData();
        data.SaveTransform(transform);
        data.SpawnableIDType = SpawnableIDType.Lift;
        cage.SaveTo(data);
        return data;
    }

    [System.Serializable]
    public class LiftSaveData : SpawnableSaveData
    {
        public SerializedVector3 CagePosition;
        public float CageDistance;
        public float CageVelocity;
        public LiftState CageState;
    }
}
