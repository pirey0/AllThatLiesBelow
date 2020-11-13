using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour, IEntity
{
    [SerializeField] float width = 2;
    [SerializeField] float destructionSpeed;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out IEntity entity))
        {
            float speed = collision.relativeVelocity.magnitude;
            Debug.Log("Entity: " + entity);
            //Damage
        }
        else if (collision.collider.TryGetComponent(out GridElement gridElement))
        {
            if (gridElement.TileMap == null || collision.relativeVelocity.magnitude < destructionSpeed)
            {
                return;
            }

            Vector3[] offsets = { new Vector3(0,-1), new Vector3(-0.55f,-1), new Vector3(0.55f, -1) };

            foreach (var offset in offsets)
            {
                var pos = (transform.position + offset).ToGridPosition();
                Util.DebugDrawTile(pos);
                bool block = gridElement.TileMap.IsBlockAt(pos.x, pos.y);
                if (block)
                {
                    gridElement.TileMap.DamageAt(pos.x, pos.y, 100);
                }
            }

        }
    }
}

