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
        data.SpawnableIDType = SpawnableIDType.Decoration;
        int index = (sprites.Contains(spriteRenderer.sprite)) ? sprites.IndexOf(spriteRenderer.sprite) : 0;
        data.index = index;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is NonPersistentDecorationSaveData data)
        {
            spriteRenderer.sprite = sprites[data.index];
        }
    }

    [System.Serializable]
    public class NonPersistentDecorationSaveData : SpawnableSaveData
    {
        public int index;
    }
}
