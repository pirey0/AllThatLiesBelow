using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : TilemapCarvingEntity, INonPersistantSavable
{
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] AudioSource fallSound;
    [SerializeField] GameObject topCollider, botCollider;
    [SerializeField] int layerUse, layerNormal;

    public void NotifyUse()
    {
        botCollider.layer = layerUse;
        topCollider.layer = layerUse;
    }

    public void NotifyLeave()
    {
        botCollider.layer = layerNormal;
        topCollider.layer = layerNormal;
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

    public SpawnableSaveData ToSaveData()
    {
        var data = new SpawnableSaveData();
        data.SpawnableIDType = SpawnableIDType.Ladder;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);

        return data;
    }

    public void Load(SpawnableSaveData data)
    {
    }
}
