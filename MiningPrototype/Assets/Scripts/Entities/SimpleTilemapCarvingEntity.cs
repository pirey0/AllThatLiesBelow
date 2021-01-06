using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTilemapCarvingEntity : TilemapCarvingEntity
{
    void Start()
    {
        Carve();
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (reason == TileUpdateReason.Destroy)
            UncarveDestroy();
    }
}
