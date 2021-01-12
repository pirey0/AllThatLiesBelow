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
    int visualHeight = 0;
    Coroutine adaptHeightRoutine;

    private void Start()
    {
        UpdateHeight();
    }

    private void UpdateHeight()
    {
        Debug.Log("Updating Rope Height");
        if (adaptHeightRoutine != null)
            StopCoroutine(adaptHeightRoutine);

        height = Mathf.Min(maxHeight, MapHelper.AirTileCount(map, (transform.position + Vector3.down).ToGridPosition(), Direction.Down, TileType.Rope));
        SetTilesToOccupy();
        adaptHeightRoutine = StartCoroutine(AdaptHeightRoutine());
        Carve();
    }

    IEnumerator AdaptHeightRoutine()
    {
        while (visualHeight < height)
        {
            visualHeight++;
            SetHeight(visualHeight);
            yield return new WaitForSeconds(delayBetweenHeightIncrease);
        }
    }

    private void SetTilesToOccupy()
    {
        tilesToOccupy = new TileOffsetTypePair[height];
        for (int i = 0; i < height; i++)
        {
            Vector2Int pos = new Vector2Int(0, -i - 1);
            tilesToOccupy[i] = new TileOffsetTypePair(pos, TileType.Rope);
        }
    }

    private void SetHeight(int newHeight)
    {
        boxCollider2D.size = new Vector2(boxCollider2D.size.x, newHeight);
        boxCollider2D.offset = new Vector2(0, -newHeight / 2);
        spriteRenderer.size = new Vector2(spriteRenderer.size.x, newHeight);
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (reason == TileUpdateReason.Destroy)
        {
            Vector2Int pos = transform.position.ToGridPosition();
            if (y == pos.y - 1)
            {
                UncarveDestroy();
            }
            else
            {
                visualHeight = pos.y - y - 2;
                UpdateHeight();
            }
        }
    }

    public override void OnTileUpdated(int x, int y)
    {
        if (isbeingDestroyed)
            return;

        Vector2Int pos = transform.position.ToGridPosition();
        Util.DebugDrawTile(pos + new Vector2Int(0, -height - 1));

        if (map.IsAirAt(pos.x, pos.y))
        {
            UncarveDestroy();
        }
        else if (map.IsAirAt(pos.x, pos.y - height - 1))
        {
            UpdateHeight();
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
