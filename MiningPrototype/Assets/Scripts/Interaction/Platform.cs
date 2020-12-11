using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : PlatformBase
{
    [SerializeField] EdgeCollider2D edgeCollider;


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


    public override void AdaptPlacementTo((Direction, int) placementAtr)
    {
        base.AdaptPlacementTo(placementAtr);

        if (placementAtr.Item1 == Direction.Right)
        {
            edgeCollider.SetPoints(new List<Vector2>() { new Vector2(-0.5f, 0), new Vector2(placementAtr.Item2 - 0.5f, 0) });
        }
        else
        {
            edgeCollider.SetPoints(new List<Vector2>() { new Vector2(-placementAtr.Item2 + 0.5f, 0), new Vector2(+0.5f, 0) });
        }

    }
}