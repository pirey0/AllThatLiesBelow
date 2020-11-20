using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : TilemapCarvingEntity, ITileMapElement
{
    [SerializeField] EdgeCollider2D edge;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] AudioSource fallSound;

    public Map TileMap { get; private set; }

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

    public override void OnTileUpdated(int x, int y, TileUpdateReason reason)
    {
        if (this == null || reason != TileUpdateReason.Destroy)
            return;

        UncarveDestroy();
        Debug.Log("Destroying ladder " + reason);
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
        if (fallSound != null)
            fallSound.Play();

        while (!rigidbody.IsSleeping())
        {
            yield return null;
        }

        if (fallSound != null)
             fallSound?.Stop();

            rigidbody.isKinematic = true;
        Carve();
    }

    public void Setup(Map tileMap)
    {
        TileMap = tileMap;
    }
}
