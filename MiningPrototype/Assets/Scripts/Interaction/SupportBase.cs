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

        tilesToOccupy = new  TileOffsetTypePair[height + 2];
        tilesToOccupy[0] = new TileOffsetTypePair(0, 0, TileType.CollapsableEntity);

        for (int i = 1; i < height; i++)
        {
            tilesToOccupy[i] = new TileOffsetTypePair(0, i, TileType.FloatingEntity);
        }

        tilesToOccupy[height ] = new  TileOffsetTypePair(-1, height-1, TileType.FloatingEntity);
        tilesToOccupy[height +1] = new TileOffsetTypePair(1, height-1, TileType.FloatingEntity);
    }

    public int CalculateHeight()
    {
        return Mathf.Min(maxHeight, MapHelper.AirTileCountAbove(RuntimeProceduralMap.Instance, transform.position.ToGridPosition(), entitiesAsAir: true));
    }
}