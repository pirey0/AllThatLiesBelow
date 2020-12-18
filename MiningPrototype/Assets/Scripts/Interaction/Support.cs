using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase
{
    [SerializeField] Vector2Int stabilityRange;

    private void Start()
    {
        var height = CalculateHeight();
        AdaptHeightTo(height);
        Debug.Log("Support placed with height: " + height);
        Carve();

        StabilizeAt(height);
    }

    private void StabilizeAt(int height)
    {
        Vector2Int center = (transform.position + new Vector3(0,height,0)).ToGridPosition();

        for (int y = stabilityRange.x; y <= stabilityRange.y; y++)
        {
            for (int x = stabilityRange.x; x <= stabilityRange.y; x++)
            {
                Vector2Int pos = new Vector2Int(center.x + x, center.y + y);
                Util.DebugDrawTile(pos);
                map.TryRemoveUnstableAt(pos);
            }
        }
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
