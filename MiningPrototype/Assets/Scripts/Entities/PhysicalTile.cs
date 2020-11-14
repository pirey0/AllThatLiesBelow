using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IEntity 
{

}

public class PhysicalTile : MineableObject, IEntity
{
    [SerializeField] SpriteRenderer renderer;

    private TileMap generator;

    public void Setup( TileMap testGeneration)
    {
        generator = testGeneration;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.TryGetComponent(out IEntity entity))
        {
            //Damage?
        }
        else if(collision.transform.TryGetComponent(out GridElement element))
        {
            var position = transform.position.ToGridPosition();
            generator.PlaceAt(position.x, position.y);
            Util.DebugDrawTile(position);
            Destroy(gameObject);
        }
    }
}
