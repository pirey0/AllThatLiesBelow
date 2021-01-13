using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeablePainting : MineableObject, INonPersistantSavable, IBaseInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] painting;

    [SerializeField] AudioSource switchPaintingSound;
    [Zenject.Inject] CameraController cameraController;

    [SerializeField] int currentPaintingId;

    private void Start()
    {
        spriteRenderer.sortingOrder = 10000 - (int)transform.position.y;
    }

    public void BeginInteracting(GameObject interactor)
    {
        TryNextPainting();
    }

    [Button]
    private void TryNextPainting()
    {
        if (spriteRenderer == null || painting == null || painting.Length <= 0)
        {
            Debug.LogError("missing references on painting " + name);
            return;
        }

        while (!TryDisplay(currentPaintingId + 1))
        {
            currentPaintingId = -1;
        }
        currentPaintingId++;

        switchPaintingSound.Play();
        cameraController?.Shake(transform.position, CameraShakeType.explosion, 0.1f, 10, 0.25f);
    }

    bool TryDisplay(int id)
    {
        if (id < painting.Length && painting[id] != null)
        {
            spriteRenderer.sprite = painting[id];
            return true;
        }
        else
        {
            return false;
        }
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new CustomizeablePaintingSaveData();
        data.SaveTransform(transform);
        data.paintingId = currentPaintingId;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is CustomizeablePaintingSaveData data)
        {
            currentPaintingId = data.paintingId;
            if (!TryDisplay(currentPaintingId))
                Debug.Log("painting: invalid symbol index " + currentPaintingId);
        }
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID("painting");
    }

    [System.Serializable]
    public class CustomizeablePaintingSaveData : SpawnableSaveData
    {
        public int paintingId;
    }
}
