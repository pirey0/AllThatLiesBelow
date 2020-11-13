using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PhysicalTile : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;

    private TileMap generator;

    public void Setup( TileMap testGeneration)
    {
        generator = testGeneration;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        var position = transform.position.ToGridPosition();
        generator.PlaceAt(position.x, position.y);
        Destroy(gameObject);
    }
}
