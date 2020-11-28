using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : TilemapCarvingEntity
{
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] AudioSource fallSound;
    [SerializeField] GameObject topCollider, botCollider;
    [SerializeField] int layerUse, layerNormal;

    public void NotifyUse()
    {
        if (botCollider != null && topCollider != null)
        {
            botCollider.layer = layerUse;
            topCollider.layer = layerUse;
        }
    }

    public void NotifyLeave()
    {
        if (botCollider != null && topCollider != null)
        {
            botCollider.layer = layerNormal;
            topCollider.layer = layerNormal;
        }
    }


    protected void Start()
    {
        Carve();
        rigidbody.isKinematic = true;
        rigidbody.velocity = Vector2.zero;
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
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
            rigidbody.WakeUp();
            StartCoroutine(FallingRoutine());
        }
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

        if (rigidbody != null)
            rigidbody.isKinematic = true;
        Carve();
    }
}
