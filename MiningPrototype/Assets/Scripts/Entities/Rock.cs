using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : TilemapCarvingEntity
{
    [SerializeField] float width = 2;
    [SerializeField] float destructionSpeed;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] AudioSource rockFalling;
    [SerializeField] AudioSource rockSmashing;
    [SerializeField] float crumbleMinTime = 0.3f;
    [SerializeField] SpriteRenderer renderer;

    float lastCrumbleStamp = -1000;

    protected void Start()
    {
        Carve();
        rigidbody.isKinematic = true;
    }


    public override void OnTileCrumbleNotified(int x, int y)
    {
        if (this == null)
            return;

        Debug.Log("Crumble " + (Time.time - lastCrumbleStamp));

        if (Time.time - lastCrumbleStamp < crumbleMinTime)
        {
            TryDestroyBelow(RuntimeProceduralMap.Instance); //<-dirty
            Debug.Log("Instant Crumble!");
        }

        lastCrumbleStamp = Time.time;
        UnCarvePrevious();
        rigidbody.isKinematic = false;
        rigidbody.WakeUp();
        StartCoroutine(FallingRoutine());

    }


    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if (reason == TileUpdateReason.Destroy || reason == TileUpdateReason.Generation)
        {
            UncarveDestroy();
        }
    }

    public override void OnTileUpdated(int x, int y, TileUpdateReason reason)
    {
        Tile t = RuntimeProceduralMap.Instance[x, y];
        if (t.Visibility > 2)
            renderer.enabled = false;
        else
            renderer.enabled = true;
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
        float speed = collision.relativeVelocity.magnitude;

        if (!rockSmashing.isPlaying && speed >= 7)
            rockSmashing.Play();

        if (collision.collider.TryGetComponent(out IEntity entity))
        {
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

            TryDestroyBelow(gridElement.TileMap);
        }
    }

    private void TryDestroyBelow(BaseMap map)
    {
        Vector3[] offsets = { new Vector3(0, -1), new Vector3(-0.55f, -1), new Vector3(0.55f, -1) };

        foreach (var offset in offsets)
        {
            var pos = (transform.position + offset).ToGridPosition();
            Util.DebugDrawTile(pos);
            bool block = map.IsBlockAt(pos.x, pos.y);
            if (block)
            {
                map.DamageAt(pos.x, pos.y, 100, playerCaused: false);
            }
        }
    }

}

