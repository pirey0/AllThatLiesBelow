using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlatformBase : TilemapCarvingEntity
{
    [SerializeField] protected SpriteRenderer renderer;
    [SerializeField] protected int minWidth, maxWidth;

    public virtual void AdaptPlacementTo((Direction, int) placementAtr)
    {
        renderer.size = new Vector2((placementAtr.Item1 == Direction.Left)? -placementAtr.Item2:placementAtr.Item2, 2);
        tilesToOccupy = new TileOffsetTypePair[placementAtr.Item2];

        for (int i = 0; i < tilesToOccupy.Length; i++)
        {
            Vector2Int offset = i * placementAtr.Item1.AsV2Int();
            TileType type = (i == 0 || i == tilesToOccupy.Length - 1) ? TileType.CollapsableEntityNotNeighbour : TileType.Platform;

            tilesToOccupy[i] = new TileOffsetTypePair(offset.x, offset.y, type);
        }
    }

    public (Direction, int) CalculatePlacement()
    {
        int wL = MapHelper.AirTileCount(RuntimeProceduralMap.Instance, transform.position.ToGridPosition(), Direction.Left, entitiesAsAir: false);
        int wR = MapHelper.AirTileCount(RuntimeProceduralMap.Instance, transform.position.ToGridPosition(), Direction.Right, entitiesAsAir: false);

        Direction dir = wL > wR ? Direction.Left : Direction.Right;
        int size = Mathf.Max(wL, wR);


        return (dir, size);
    }

}