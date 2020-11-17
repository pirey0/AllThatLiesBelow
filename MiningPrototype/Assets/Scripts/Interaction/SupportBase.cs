using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SupportBase : TilemapCarvingEntity
{
    [SerializeField] protected SpriteRenderer front, back;
    [SerializeField] protected int maxHeight;

    public void AdaptHeightTo(int height)
    {
        float spriteHeight = (float)height * (10f / 7.5f);

        front.size = new Vector2(3, spriteHeight);
        back.size = new Vector2(3, spriteHeight);

        tilesToOccupy = new Vector2Int[height];
        for (int i = 0; i < height; i++)
        {
            tilesToOccupy[i] = new Vector2Int(0, i);
        }
    }

    public int CalculateHeight()
    {
        return Mathf.Min(maxHeight, TileMapHelper.AirTileCountAbove(TileMap.Instance, transform.position.ToGridPosition()));
    }

}