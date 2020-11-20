using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : TilemapCarvingEntity, ITileMapElement
{
    [SerializeField] float width = 2;
    [SerializeField] float destructionSpeed;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] AudioSource rockFalling;
    [SerializeField] AudioSource rockSmashing;

    public Map TileMap { get; private set; }

    protected void Start()
    {
        Carve();
        rigidbody.isKinematic = true;
    }


    public override void OnTileCrumbleNotified(int x, int y)
    {
        if (this == null)
            return;

        UnCarvePrevious();
        rigidbody.isKinematic = false;
        rigidbody.WakeUp();
        StartCoroutine(FallingRoutine());
    }

    public override void OnTileUpdated(int x, int y, TileUpdateReason reason)
    {
        if(reason == TileUpdateReason.Destroy || reason == TileUpdateReason.Generation)
        {
            UncarveDestroy();
        }
    }

    private IEnumerator FallingRoutine()
    {
        rockFalling?.Play();

        while (!rigidbody.IsSleeping())
        {
            yield return null;
        }

        rockFalling?.Stop();

        rigidbody.isKinematic = true;
        Carve();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!rockSmashing.isPlaying)
            rockSmashing.Play();

        if (collision.collider.TryGetComponent(out IEntity entity))
        {
            float speed = collision.relativeVelocity.magnitude;
            float angle = Mathf.Acos(Vector2.Dot(collision.contacts[0].normal, Vector2.up)) * Mathf.Rad2Deg;

            //Debug.Log(angle + " " + speed);
            if (angle < 70 && !rigidbody.isKinematic)
            {
                entity.TakeDamage(DamageStrength.Strong);
                rigidbody.velocity = collision.relativeVelocity * -1;
            }
        }
        else if (collision.collider.TryGetComponent(out ITileMapElement gridElement))
        {
            if (gridElement.TileMap == null || collision.relativeVelocity.magnitude < destructionSpeed)
            {
                return;
            }

            Vector3[] offsets = { new Vector3(0, -1), new Vector3(-0.55f, -1), new Vector3(0.55f, -1) };

            foreach (var offset in offsets)
            {
                var pos = (transform.position + offset).ToGridPosition();
                Util.DebugDrawTile(pos);
                bool block = gridElement.TileMap.IsBlockAt(pos.x, pos.y);
                if (block)
                {
                    gridElement.TileMap.DamageAt(pos.x, pos.y, 100, playerCaused: false);
                }
            }

        }
    }

    public void Setup(Map tileMap)
    {
        TileMap = tileMap;
    }
}

