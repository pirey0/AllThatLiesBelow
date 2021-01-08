using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : TilemapCarvingEntity
{
    [SerializeField] EdgeCollider2D edgeCollider;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite farLeft, left, center, centerDot, right, farRight;
    int fLeft, nLeft, nRight, fRight;

    private void Start()
    {
        //var placmentAttr = CalculatePlacement();
        //AdaptPlacementTo(placmentAttr);
        //Debug.Log("Support placed with height: " + placmentAttr);
        Carve();

        var pos = transform.position.ToGridPosition();
        map.NotifyRecieversOfUpdates(pos.x + 1, pos.y, TileUpdateReason.VisualUpdate);
        map.NotifyRecieversOfUpdates(pos.x - 1, pos.y, TileUpdateReason.VisualUpdate);
        spriteRenderer.sprite = UpdateVisualsBaseOnNeighbours(pos.x, pos.y);
    }

    public override void OnTileUpdated(int x, int y)
    {
        if (this != null)
        {
            spriteRenderer.sprite = UpdateVisualsBaseOnNeighbours(x, y);
        }
    }

    private Sprite UpdateVisualsBaseOnNeighbours(int x, int y)
    {
        fLeft = (map.IsNeighbourAt(x - 2, y) ? 4 : 0);
        fRight = (map.IsNeighbourAt(x + 2, y) ? 4 : 0);
        nLeft = (map.IsBlockAt(x - 1, y) ? 1 : 0) + (map.IsNeighbourAt(x - 1, y) ? 2 : 0);
        nRight = (map.IsBlockAt(x + 1, y) ? 1 : 0) + (map.IsNeighbourAt(x + 1, y) ? 2 : 0);

        if ((nRight + nLeft) == 0)
            UncarveDestroy();

        if ((nLeft + fLeft) > 1)
        {
            if (nLeft == 1 || nRight == 0)
            {
                return this.left;
            }
            else if (nLeft != 0)
            {
                return this.farLeft;
            }

        }
        else if ((nRight + fRight) > 1)
        {
            if (nRight == 1 || nLeft == 0)
            {
                return this.right;
            }
            else if (nRight != 0)
            {
                return this.farRight;
            }
        }
        else if (nLeft + nRight < 2)
        {
            return this.centerDot;
        }

        return this.center;
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
        //UncarveDestroy();
        //Platform broke... What happens now?
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position, fLeft + "<" + nLeft + " - " + nRight + ">" + fRight + "\n  == " + ((fLeft + nLeft) - (nRight + fLeft)) + " ==");
    }

#endif


    //public override void AdaptPlacementTo((Direction, int) placementAtr)
    //{
    //    base.AdaptPlacementTo(placementAtr);
    //
    //    if (placementAtr.Item1 == Direction.Right)
    //    {
    //        edgeCollider.SetPoints(new List<Vector2>() { new Vector2(-0.5f, 0), new Vector2(placementAtr.Item2 - 0.5f, 0) });
    //    }
    //    else
    //    {
    //        edgeCollider.SetPoints(new List<Vector2>() { new Vector2(-placementAtr.Item2 + 0.5f, 0), new Vector2(+0.5f, 0) });
    //    }
    //
    //}
}