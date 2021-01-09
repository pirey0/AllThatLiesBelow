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
    [SerializeField] SpriteRenderer overlayRenderer;
    [SerializeField] new SpriteRenderer renderer;
    [SerializeField] AudioSource hit;
    [SerializeField] GameObject onLandEffects;
    [SerializeField] float RequiredSpeedForStrongHit;

    Tile tile;
    TileInfo info;
    bool placed;

    public void Setup(RuntimeProceduralMap testGeneration, Tile tile, TileInfo info)
    {
        map = testGeneration;
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

    private void Update()
    {
        var pos = transform.position.ToGridPosition();
        Util.DebugDrawTile(pos, Color.red, 0.1f);
        
        if(map.IsBlockAt(pos.x, pos.y))
        {
            TryPlace();
        }
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
            TryPlace();
        }
    }

    private void TryPlace()
    {
        if (!placed)
        {
            var position = transform.position.ToGridPosition();

            if (map.IsBlockAt(position.x, position.y))
                position.y += 1;

            map.SetMapAt(position.x, position.y, tile, TileUpdateReason.Place);

            Util.DebugDrawTile(position, Color.green, 2);
            if (onLandEffects != null)
                Instantiate(onLandEffects); //Safe

            Destroy(gameObject);
            placed = true;
        }
    }

    public void TakeDamage(DamageStrength strength)
    {
        Destroy(gameObject);
    }
}
