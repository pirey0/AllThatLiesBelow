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

            var right = (transform.position + new Vector3(0, -2f)).ToGridPosition();
            var left = (transform.position + new Vector3(-width * 0.3f, -2f)).ToGridPosition();

            Util.DebugDrawTile(right);
            Util.DebugDrawTile(left);

            bool blockLeft = gridElement.TileMap.IsBlockAt(left.x, left.y);
            bool blockRight = gridElement.TileMap.IsBlockAt(right.x, right.y);

            if (!blockLeft || !blockRight)
            {
                if (blockRight)
                {
                    gridElement.TileMap.DamageAt(right.x, right.y, 100);
                }
                else
                {
                    gridElement.TileMap.DamageAt(left.x, left.y, 100);
                }
            }
        }
    }

}
