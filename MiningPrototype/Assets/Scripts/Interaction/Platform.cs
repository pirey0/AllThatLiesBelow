using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : TilemapCarvingEntity
{
    [SerializeField] EdgeCollider2D edgeCollider;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite farLeft, left, center, centerDot, right, farRight;

    [Zenject.Inject] PlatformHandler platformHandler;

    bool platformLeft, platformRight, wallLeft, wallRight, wallFarLeft, wallFarRight;

    private void Start()
    {
        Carve();

        var pos = transform.position.ToGridPosition();

        spriteRenderer.sprite = UpdateVisualsBaseOnNeighbours(pos.x, pos.y);

        map.NotifyRecieversOfUpdates(pos.x + 1, pos.y, TileUpdateReason.VisualUpdate);
        map.NotifyRecieversOfUpdates(pos.x - 1, pos.y, TileUpdateReason.VisualUpdate);

        platformHandler.NotifyPlatformPlaced(this);
    }

    public override void OnTileUpdated(int x, int y)
    {
        if (this != null && !isbeingDestroyed)
        {
            Util.DebugDrawTileCrossed(new Vector2Int(x,y),Color.green);

            platformHandler.CheckForAttachmentToWall(this);
            spriteRenderer.sprite = UpdateVisualsBaseOnNeighbours(x, y);
        }
    }

    private Sprite UpdateVisualsBaseOnNeighbours(int x, int y)
    {
        
        platformLeft = map.GetTileAt(x - 1, y).Type == TileType.Platform;
        platformRight = map.GetTileAt(x + 1, y).Type == TileType.Platform;
        wallLeft = map.IsNeighbourAt(x - 1, y);
        wallRight = map.IsNeighbourAt(x + 1, y);
        wallFarLeft = map.IsNeighbourAt(x - 2, y);
        wallFarRight = map.IsNeighbourAt(x + 2, y);


        if (wallRight)
        {
            if (platformLeft)
                return this.farRight;
            else
                return this.right;

        } else if (wallLeft)
        {
            if (platformRight)
                return this.farLeft;
            else
                return this.left;
        } else if (platformLeft || platformRight)
        {
            if (wallFarRight && platformRight)
                return this.right;
            else if (wallFarLeft && platformLeft)
                return this.left;
            else if (!(platformLeft && platformRight))
                return this.centerDot;
        }

        return this.center;
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (this != null && reason == TileUpdateReason.Destroy && !isbeingDestroyed)
        {
            Debug.Log("Platform destroyed.");
            platformHandler.NotifyPlatformDestroyed(this);
            UncarveDestroy();
        }
    }

    public override void OnTileCrumbleNotified(int x, int y)
    {
        //UncarveDestroy();
        //Platform broke... What happens now?
    }

    public bool IsAdjacendTo(Platform platform)
    {
        if (transform.position.ToGridPosition().y != platform.transform.position.ToGridPosition().y)
            return false;

        return Mathf.Abs(platform.transform.position.x - transform.position.x) <= 1;
    }
    public bool HasConnectionToWall()
    {
        var pos = transform.position.ToGridPosition();
        return map.IsNeighbourAt(pos.x + 1, pos.y) || map.IsNeighbourAt(pos.x - 1, pos.y);
    }

    public int GetNumberOfNeightbours()
    {
        var pos = transform.position.ToGridPosition();
        return (map.IsBlockAt(pos.x + 1, pos.y) ? 1 : 0) + (map.IsNeighbourAt(pos.x - 1, pos.y) ? 1 : 0);
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.down * (transform.position.y % 10f) * 0.1f, wallFarLeft + " < " + platformLeft + "=wall?"+ wallLeft + " - " + platformRight + "=wall?"+wallRight + " > " + wallFarRight);
    }

#endif
}