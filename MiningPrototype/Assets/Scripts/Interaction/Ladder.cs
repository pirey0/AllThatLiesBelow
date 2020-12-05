using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : FallingTilemapCarvingEntity
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

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (this == null || reason != TileUpdateReason.Destroy)
            return;

        UncarveDestroy();
        Debug.Log("Destroying ladder " + reason);
    }
}
