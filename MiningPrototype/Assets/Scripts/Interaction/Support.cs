using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase
{
    private void Start()
    {
        var height = CalculateHeight();
        AdaptHeightTo(height);
        Debug.Log("Support placed with height: " + height);
        Carve();
    }


    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if(this != null && reason == TileUpdateReason.Destroy)
        {
            Debug.Log("Support destroyed.");
            UncarveDestroy();
        }
    }

    public override void OnTileCrumbleNotified(int x, int y)
    {
        UncarveDestroy();
        //Support broke... What happens now?
    }

}
