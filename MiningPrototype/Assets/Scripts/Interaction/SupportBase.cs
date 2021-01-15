using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SupportBase : TilemapCarvingEntity
{
    [SerializeField] protected SpriteRenderer front, back;
    [SerializeField] protected int maxHeight;

    public void AdaptHeightTo(int height)
    {
        float spriteHeight = Mathf.Max(3f,(float)height * 1.1875f);

        front.size = new Vector2(3, spriteHeight);
        back.size = new Vector2(3, spriteHeight);

        tilesToOccupy = new  TileOffsetTypePair[height + 2];
        tilesToOccupy[0] = new TileOffsetTypePair(0, 0, TileType.CollapsableEntityNotNeighbour);

        for (int i = 1; i < height; i++)
        {
            tilesToOccupy[i] = new TileOffsetTypePair(0, i, TileType.FloatingEntityNotNeighbour);
        }

        tilesToOccupy[height ] = new  TileOffsetTypePair(-1, height-1, TileType.FloatingEntityNotNeighbour);
        tilesToOccupy[height +1] = new TileOffsetTypePair(1, height-1, TileType.FloatingEntityNotNeighbour);
    }

    public int CalculateHeight()
    {
        return Mathf.Min(maxHeight, MapHelper.AirTileCountAbove(map, transform.position.ToGridPosition(), TileType.FloatingEntityNotNeighbour, TileType.CollapsableEntityNotNeighbour));
    }
}