using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TilemapCarvingEntity : MirrorWorldFollower, ITileUpdateReceiver, ITileMapElement
{
    [SerializeField] ItemAmountPair drop;
    [SerializeField] protected TileOffsetTypePair[] tilesToOccupy;
    [SerializeField] protected Vector3 carvingOffset;


    [Zenject.Inject] protected InventoryManager inventoryManager;
    [Zenject.Inject] protected RuntimeProceduralMap map;

    Vector2Int tilemapPos = new Vector2Int(-1, -1);
    protected bool isbeingDestroyed;

    public BaseMap TileMap { get; private set; }

    public virtual void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
    }

    public virtual void OnTileCrumbleNotified(int x, int y)
    {
    }

    public virtual void OnTileUpdated(int x, int y)
    {
    }

    [Button]
    protected void UpdateCarveIfNecessary()
    {
        if (CarveLocationChanged())
        {
            UnCarvePrevious();
            Carve();
        }
    }

    protected bool CarveLocationChanged()
    {
        return (transform.position + carvingOffset).ToGridPosition() != tilemapPos;
    }

    public virtual void UncarveDestroy()
    {
        if (this != null)
        {
            isbeingDestroyed = true;
            UnCarvePrevious();
            Destroy(gameObject);
            if (drop.IsValid())
                inventoryManager.PlayerCollects(drop.type, drop.amount);
        }
    }

    protected void Carve()
    {
        if (map != null)
        {
            tilemapPos = (transform.position + carvingOffset).ToGridPosition();
            foreach (var item in tilesToOccupy)
            {
                Vector2Int pos = tilemapPos + item.Offset;
                map.SetMapAt(pos.x, pos.y, Tile.Make(item.Type), TileUpdateReason.Carve, updateProperties: true, updateVisuals: true);
                map.SetReceiverMapAt(pos.x, pos.y, this);
            }
        }
        else
        {
            Debug.LogError("Tilemap undefined.");
        }
    }

    protected void UnCarvePrevious()
    {
        if (map != null && tilemapPos.x >= 0)
        {
            foreach (var item in tilesToOccupy)
            {
                Vector2Int pos = tilemapPos + item.Offset;
                map.SetMapAt(pos.x, pos.y, Tile.Air, TileUpdateReason.Uncarve, updateProperties: true, updateVisuals: true);
                map.SetReceiverMapAt(pos.x, pos.y, this);
            }
            tilemapPos = new Vector2Int(-1, -1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var disp in tilesToOccupy)
        {
            Gizmos.color = disp.Type == TileType.CollapsableEntity ? Color.yellow : Color.white;
            Util.GizmosDrawTile((transform.position + carvingOffset).ToGridPosition() + disp.Offset);
        }
    }

    public void Setup(BaseMap tileMap)
    {
        TileMap = tileMap;
    }
}

[System.Serializable]
public struct TileOffsetTypePair
{
    public Vector2Int Offset;
    public TileType Type;

    public TileOffsetTypePair(int x, int y, TileType type)
    {
        Offset = new Vector2Int(x, y);
        Type = type;
    }

    public TileOffsetTypePair(Vector2Int offset, TileType type)
    {
        Offset = offset;
        Type = type;
    }
}
