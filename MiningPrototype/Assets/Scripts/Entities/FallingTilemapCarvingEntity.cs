using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTilemapCarvingEntity : TilemapCarvingEntity
{
    [Header("Falling Carving Entity")]
    [SerializeField] protected new Rigidbody2D rigidbody;
    [SerializeField] AudioSource fallSound;

    protected virtual void Start()
    {
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector2.zero;
        }
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
