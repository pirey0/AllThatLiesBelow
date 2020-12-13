using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public enum DamageStrength { Weak, Strong }

public interface IEntity
{
    void TakeDamage(DamageStrength strength);
}

public class PhysicalTile : MineableObject, IEntity
{
    [SerializeField] SpriteRenderer renderer, overlayRenderer;
    [SerializeField] AudioSource hit;
    [SerializeField] GameObject onLandEffects;
    [SerializeField] float RequiredSpeedForStrongHit;

    private RuntimeProceduralMap generator;
    Tile tile;
    TileInfo info;

    public void Setup(RuntimeProceduralMap testGeneration, Tile tile, TileInfo info)
    {
        generator = testGeneration;
        this.tile = tile;
        this.info = info;

        renderer.sprite = info.UseTilesFromOtherInfo ? info.TileSourceInfo.physicalTileSprite : info.physicalTileSprite;
        overlayRenderer.sprite = info.physicalTileOverlay;
        overlayAnimator.ActiveUpdate(tile.Damage / 10);
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
        if (info.ItemToDrop != ItemType.None)
            inventoryManager.PlayerCollects(info.ItemToDrop, 1);

        Destroy(gameObject);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out IEntity entity))
        {
            if (transform.position.y > collision.transform.position.y)
            {
                Debug.Log("Physical Tile hit with speed: " + collision.relativeVelocity.y);
                if (collision.relativeVelocity.y > RequiredSpeedForStrongHit)
                {
                    Debug.Log("Strong Bonk!");
                    entity.TakeDamage(DamageStrength.Strong);
                }
                else
                {
                    Debug.Log("Weak Bonk!");
                    entity.TakeDamage(DamageStrength.Weak);
                }

                hit?.Play();
            }
        }
        else if (collision.transform.TryGetComponent(out ITileMapElement element))
        {
            var position = transform.position.ToGridPosition();

            if (generator.IsBlockAt(position.x, position.y))
                position.y += 1;

            generator.SetMapAt(position.x, position.y, tile, TileUpdateReason.Place);

            Util.DebugDrawTile(position);
            Instantiate(onLandEffects);
            Destroy(gameObject);
        }
    }

    public void TakeDamage(DamageStrength strength)
    {
        Destroy(gameObject);
    }
}
