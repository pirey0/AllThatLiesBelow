using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClimbable
{
    void NotifyUse();
    void NotifyLeave();
    Vector3 GetTopPosition();
    Vector3 GetBottomPosition();
    float GetHeight();
}

public class Ladder : FallingTilemapCarvingEntity, IClimbable
{
    [SerializeField] GameObject topCollider, botCollider;
    [SerializeField] int layerUse, layerNormal;

    public void NotifyUse()
    {
        if (botCollider != null && topCollider != null)
        {
            botCollider.layer = layerUse;
            topCollider.layer = layerUse;
        }
    }

    public void NotifyLeave()
    {
        if (botCollider != null && topCollider != null)
        {
            botCollider.layer = layerNormal;
            topCollider.layer = layerNormal;
        }
    }

    protected override void Start()
    {
        base.Start();
        Carve();
    }

    public Vector3 GetPosition ()
    {
        return transform.position;
    }

    public Vector3 GetTopPosition()
    {
        return transform.position + GetHeight() * Vector3.up;
    }
    public Vector3 GetBottomPosition()
    {
        return transform.position;
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (this == null || reason != TileUpdateReason.Destroy)
            return;

        UncarveDestroy();
        Debug.Log("Destroying ladder " + reason);
    }

    public float GetHeight()
    {
        return 5.5f; //hardcoded ladder height
    }
}
