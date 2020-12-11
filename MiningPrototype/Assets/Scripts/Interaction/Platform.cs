using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : PlatformBase
{
    private void Start()
    {
        var placmentAttr = CalculatePlacement();
        AdaptPlacementTo(placmentAttr);
        Debug.Log("Support placed with height: " + placmentAttr);
        Carve();
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (this != null && reason == TileUpdateReason.Destroy)
        {
            Debug.Log("Support destroyed.");
            UncarveDestroy();
        }
    }

    public override void OnTileCrumbleNotified(int x, int y)
    {
        UncarveDestroy();
        //Platform broke... What happens now?
    }
}