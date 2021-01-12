using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NonPersistentDecoration : MirrorWorldFollower, INonPersistantSavable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] List<Sprite> sprites;

    [Header("Sprite Setter")]
    [SerializeField] Sprite sprite;

    [Button]
    private void SetSprite()
    {
        spriteRenderer.sprite = sprite;
        if (!sprites.Contains(sprite))
            sprites.Add(sprite);
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new NonPersistentDecorationSaveData();
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);

        if (IsDefined(spriteRenderer.sprite))
        {
            data.index = sprites.IndexOf(spriteRenderer.sprite);
        }
        else
        {
            data.index = -1;
            Debug.LogError("No index defined for " + spriteRenderer.sprite.name);
        }
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
                spriteRenderer.sprite = sprites[data.index];
            }
        }
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID(SpawnableIDType.Decoration);
    }

    [System.Serializable]
    public class NonPersistentDecorationSaveData : SpawnableSaveData
    {
        public int index;
    }
}
