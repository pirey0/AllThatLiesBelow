using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IEntity
{

}

public class PhysicalTile : MineableObject, IEntity
{
    [SerializeField] SpriteRenderer renderer, overlayRenderer;

    private TileMap generator;
    Tile tile;
    TileInfo info;

    public void Setup(TileMap testGeneration, Tile tile, TileInfo info)
    {
        generator = testGeneration;
        this.tile = tile;
        this.info = info;

        renderer.sprite = info.UseTilesFromOtherInfo ? info.TileSourceInfo.physicalTileSprite : info.physicalTileSprite;
        overlayRenderer.sprite = info.physicalTileOverlay;
        overlayAnimator.ActiveUpdate(tile.Damage/10);
    }

    public override void Damage(float v)
    {
        tile.TakeDamage(v);

        //Not working correctly v
        overlayAnimator.ActiveUpdate(v);

        if (tile.Damage >= 10)
            Destroyed();
    }

    protected override void Destroyed()
    {
        InventoryManager.PlayerCollects(info.ItemToDrop, contains.amount);
        Destroy(gameObject);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out IEntity entity))
        {
            //Damage?
        }
        else if (collision.transform.TryGetComponent(out TileMapElement element))
        {
            var position = transform.position.ToGridPosition();
            generator.PlaceAt(position.x, position.y, tile);
            Util.DebugDrawTile(position);
            Destroy(gameObject);
        }
    }
}
