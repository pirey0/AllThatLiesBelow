using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : TilemapCarvingEntity, INonPersistantSavable
{
    [SerializeField] LiftCage cage;
    [SerializeField] LineRenderer lineRenderer;

    int height;

    private void Start()
    {
        Carve();
        RecalcuateHeight();

    }

    public int GetHeight()
    {
        return height;
    }

    public void RecalcuateHeight()
    {
        Debug.Log("Recalculating Lift Height");
        var gridPos = transform.position.ToGridPosition();
        int min = int.MaxValue;

        for (int x = -1; x <= 2; x++)
        {
            int c = MapHelper.AirTileCount(map, new Vector2Int(gridPos.x + x, gridPos.y - 2), Direction.Down, false);
            Util.DebugDrawTile(new Vector2Int(gridPos.x + x, gridPos.y - 2), Color.yellow, 1f);

            if(c < min)
                min = c;
        }

        height = min;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] {transform.position, transform.position + Vector3.down*height});
    }
    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (reason == TileUpdateReason.Destroy)
        {
            UncarveDestroy();
        }
    }

    public void Load(SpawnableSaveData data)
    {
        if (data is LiftSaveData sdata)
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
