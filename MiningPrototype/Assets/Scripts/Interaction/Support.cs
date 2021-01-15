using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase
{
    [SerializeField] Vector2Int stabilityRange;
    [SerializeField] Sprite[] frontSprites, backSprites;

    private void Start()
    {
        var height = CalculateHeight();
        AdaptHeightTo(height);

        //adapt visuals to ceiling situation
        Vector2Int ceilingPos = transform.position.ToGridPosition() + Vector2Int.up * height;
        bool ceilingRightIsAir = map.IsAirAt(ceilingPos.x + 1, ceilingPos.y);
        bool ceilingLeftIsAir = map.IsAirAt(ceilingPos.x - 1, ceilingPos.y);
        AdaptSpriteTo(ceilingRightIsAir, ceilingLeftIsAir);

        Carve();

        StabilizeAt(height);
    }

    private void AdaptSpriteTo(bool ceilingRightIsAir, bool ceilingLeftIsAir)
    {
        // 0 no // 1 right // 2 left // 3 both
        int index = (ceilingRightIsAir ? 0 : 1) + (ceilingLeftIsAir ? 0 : 2);
        front.sprite = frontSprites[index];
        back.sprite = backSprites[index];
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
