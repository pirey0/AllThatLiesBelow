using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    ROCKS,
    GOLD,
    COPPER,
    FAMILY_PHOTO
}

[CreateAssetMenu]
public class ItemInformationStorage : ScriptableObject
{
    [SerializeField] List<ItemSpritePair> itemSpritePair = new List<ItemSpritePair>();
    [SerializeField] Sprite noSpriteFoundSprite;

    public Sprite GetSpriteByItemType(ItemType itemType)
    {
        foreach (ItemSpritePair pair in itemSpritePair)
        {
            if (pair.type == itemType)
                return pair.sprite;
        }

        return noSpriteFoundSprite;
    }
}

[System.Serializable]
public class ItemSpritePair
{
    public ItemType type;
    public Sprite sprite;
}
