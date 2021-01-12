using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class DecorationalVine : MirrorWorldFollower, INonPersistantSavable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] List<Sprite> sprites;

    [Header("Sprite")]
    [SerializeField] int currentSpriteId;

    [Button]
    private void Next()
    {
        if (spriteRenderer == null || sprites == null || sprites.Count <= 0)
        {
            Debug.LogError("missing references on vine " + name);
            return;
        }

        while (!TrySetSprite(currentSpriteId + 1))
        {
            currentSpriteId = -1;
        }
        currentSpriteId++;
    }

    [Button]
    private void Before()
    {
        if (spriteRenderer == null || sprites == null || sprites.Count <= 0)
        {
            Debug.LogError("missing references on vine " + name);
            return;
        }

        while (!TrySetSprite(currentSpriteId - 1))
        {
            currentSpriteId = sprites.Count;
        }
        currentSpriteId--;
    }

    private bool TrySetSprite(int id)
    {
        if (id >= 0 && id < sprites.Count && sprites[id] != null)
        {
            spriteRenderer.sprite = sprites[id];
            return true;
        }
        else
        {
            return false;
        }
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new NonPersistentDecorationSaveData();
        data.SaveTransform(transform);
        data.index = currentSpriteId;
        return data;
    }

    public bool IsDefined(Sprite sprite)
    {
        foreach (var s in sprites)
        {
            if (sprite.Equals(s))
                return true;
        }
        return false;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is NonPersistentDecorationSaveData data)
        {
            if (data.index < 0)
            {
                Debug.LogWarning("Decoration: invalid index");
            }
            else
            {
                currentSpriteId = data.index;
                spriteRenderer.sprite = sprites[currentSpriteId];
            }
        }
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID("DecoVine");
    }

    [System.Serializable]
    public class NonPersistentDecorationSaveData : SpawnableSaveData
    {
        public int index;
    }
}

