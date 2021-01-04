using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeableSign : MineableObject, INonPersistantSavable, IBaseInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer symbolRenderer;
    [SerializeField] Sprite[] symbols;

    [SerializeField] AudioSource switchSymbolSound;
    [Zenject.Inject] CameraController cameraController;

    [SerializeField] int currentSymbolId;

    private void Start()
    {
        spriteRenderer.sortingOrder = 10000 - (int)transform.position.y;
    }

    public void BeginInteracting(GameObject interactor)
    {
        TryNextSymbol();
    }

    [Button]
    private void TryNextSymbol()
    {
        if (symbolRenderer == null || symbols == null || symbols.Length <= 0)
        {
            Debug.LogError("missing references on sign " + name);
            return;
        }

        while (!TryDisplay(currentSymbolId + 1))
        {
            currentSymbolId = -1;
        }
        currentSymbolId++;

        switchSymbolSound.Play();
        cameraController.Shake(transform.position, CameraShakeType.explosion, 0.1f, 10, 0.25f);
    }

    bool TryDisplay(int id)
    {
        if (id < symbols.Length && symbols[id] != null)
        {
            symbolRenderer.sprite = symbols[id];
            return true;
        } else
        {
            return false;
        }
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new CustomizeableSignSaveData();
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.SpawnableIDType = SpawnableIDType.Sign;
        data.symbolId = currentSymbolId;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is CustomizeableSignSaveData data)
        {
            currentSymbolId = data.symbolId;
            if (!TryDisplay(currentSymbolId))
                Debug.Log("Sign: invalid symbol index " + currentSymbolId);
        }
    }

    [System.Serializable]
    public class CustomizeableSignSaveData : SpawnableSaveData
    {
        public int symbolId;
    }
}
