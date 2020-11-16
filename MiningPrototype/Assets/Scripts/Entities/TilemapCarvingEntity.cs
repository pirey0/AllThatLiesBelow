using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TilemapCarvingEntity : MonoBehaviour, ITileUpdateReceiver, IEntity
{
    [SerializeField] TileType type = TileType.CarvedEntity;
    [SerializeField] Vector2Int[] tilesToOccupy = new Vector2Int[] { Vector2Int.zero };

    Vector2Int tilemapPos = new Vector2Int(-1, -1);

    public virtual void OnTileUpdated(int x, int y, Tile newTile)
    {
        if (newTile.Type != type && gameObject != null)
        {
            Debug.Log(name + " representative tile at (" + x + "/" + y + ") destroyed. Destroying self");
            Destroy(gameObject);
        }
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
        return transform.position.ToGridPosition() != tilemapPos;
    }


    protected void Carve()
    {
        if (TileMap.Instance != null)
        {
            tilemapPos = transform.position.ToGridPosition();
            foreach (var item in tilesToOccupy)
            {
                Vector2Int pos = tilemapPos + item;
                TileMap.Instance.SetMapAt(pos.x, pos.y, Tile.Make(type), updateProperties: true, updateVisuals: true);
                TileMap.Instance.SetReceiverMapAt(pos.x, pos.y, this);
            }
        }
        else
        {
            Debug.LogError("Tilemap undefined.");
        }
    }

    protected void UnCarvePrevious()
    {
        if (TileMap.Instance != null && tilemapPos.x >= 0)
        {
            foreach (var item in tilesToOccupy)
            {
                Vector2Int pos = tilemapPos + item;
                TileMap.Instance.SetMapAt(pos.x, pos.y, Tile.Air, updateProperties: true, updateVisuals: true);
                TileMap.Instance.SetReceiverMapAt(pos.x, pos.y, this);
            }
            tilemapPos = new Vector2Int(-1, -1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var disp in tilesToOccupy)
        {
            Util.GizmosDrawTile(transform.position.ToGridPosition() + disp);
        }
    }

}
