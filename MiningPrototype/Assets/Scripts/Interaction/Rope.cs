using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : TilemapCarvingEntity, IClimbable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] int maxHeight;
    [SerializeField] float delayBetweenHeightIncrease;

    int height = 0;

    private void Start()
    {
        StartCoroutine(AdaptHeightRoutine());
        Carve();
    }

    IEnumerator AdaptHeightRoutine()
    {
        while (map.IsAirAt((int)GetTopPosition().x, (int)GetTopPosition().y - height - 1) && height < maxHeight)
        {
            SetHeight(height + 1);
            yield return new WaitForSeconds(delayBetweenHeightIncrease);
        }
    }

    private void SetHeight(int newHeight)
    {
        height = newHeight;
        boxCollider2D.size = new Vector2(boxCollider2D.size.x, newHeight);
        boxCollider2D.offset = new Vector2(0, -newHeight / 2);
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, newHeight);
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if(reason == TileUpdateReason.Destroy)
        {
            UncarveDestroy();
        }
    }

    public override void OnTileUpdated(int x, int y)
    {
        Debug.Log("Updated");
        Util.DebugDrawTile(new Vector2Int(x, y+1));
        if (RuntimeProceduralMap.Instance.IsAirAt(x, y+1))
        {
            UncarveDestroy();
        }
    }

    public Vector3 GetBottomPosition()
    {
        return transform.position + Vector3.down * height;
    }

    public float GetHeight()
    {
        return height;
    }

    public Vector3 GetTopPosition()
    {
        return transform.position;
    }

    public void NotifyLeave()
    {
        //
    }

    public void NotifyUse()
    {
        //
    }
}
