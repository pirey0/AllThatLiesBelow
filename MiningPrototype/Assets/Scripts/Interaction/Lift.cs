using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : TilemapCarvingEntity, IBaseInteractable, INonPersistantSavable
{
    [SerializeField] LiftCage cage;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] new BoxCollider2D collider;
    [SerializeField] AudioSource callSource;

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
            int c = MapHelper.AirTileCount(map, new Vector2Int(gridPos.x + x, gridPos.y - 2), Direction.Down);
            Util.DebugDrawTile(new Vector2Int(gridPos.x + x, gridPos.y - 2), Color.yellow, 1f);

            if (c < min)
                min = c;
        }

        height = min;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { transform.position, transform.position + Vector3.down * height });
        collider.size = new Vector2(1, height);
        collider.offset = new Vector2(0, -0.5f - height / 2);

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

    public void BeginInteracting(GameObject interactor)
    {
        if (cage.CanBeCalled())
        {
            Debug.Log("Called Lift");
            cage.CallTo(interactor.transform.position.y);
            callSource.Play();
        }
        else
        {
            Debug.Log("Cannot call Lift when being used");
        }
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
