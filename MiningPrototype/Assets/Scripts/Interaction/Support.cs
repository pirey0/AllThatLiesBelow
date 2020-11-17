using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase
{
    private void Start()
    {
        AdaptHeightTo(CalculateHeight());
        Carve();
    }


    public override void OnTileUpdated(int x, int y, TileUpdateReason reason)
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
