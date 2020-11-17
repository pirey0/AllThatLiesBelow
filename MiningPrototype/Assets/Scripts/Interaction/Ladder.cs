using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : TilemapCarvingEntity, ITileMapElement
{
    [SerializeField] EdgeCollider2D edge;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] AudioSource fallSound;

    public TileMap TileMap { get; private set; }

    public void NotifyGoingDown()
    {
        edge.enabled = false;
    }

    public void NotifyGoingUp()
    {
        edge.enabled = true;
    }


    protected void Start()
    {
        Carve();
        rigidbody.isKinematic = true;
    }

    public override void OnTileUpdated(int x, int y, Tile newTile)
    {
        if (this == null)
            return;

        UnCarvePrevious();
        Destroy(gameObject);
        Debug.Log("Destroying ladder");
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

    private IEnumerator FallingRoutine()
    {
        fallSound?.Play();

        while (!rigidbody.IsSleeping())
        {
            yield return null;
        }

        fallSound?.Stop();

        rigidbody.isKinematic = true;
        Carve();
    }

    public void Setup(TileMap tileMap)
    {
        TileMap = tileMap;
    }
}
