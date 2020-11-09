using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum itemType
{
    ROCKS,
    GOLD,
    COPPER
}

[CreateAssetMenu]
public class ItemInformationStorage : ScriptableObject
{
    [SerializeField] List<ItemSpritePair> itemSpritePair = new List<ItemSpritePair>();
    [SerializeField] Sprite noSpriteFoundSprite;

    public Sprite GetSpriteByItemType(itemType itemType)
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
    public itemType type;
    public Sprite sprite;
}
