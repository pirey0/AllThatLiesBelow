using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SupportBase : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer front, back;
    [SerializeField] protected int maxHeight;

    public void AdaptHeightTo(int height)
    {
        float spriteHeight = (float)height * (10f / 7.5f);

        front.size = new Vector2(3, spriteHeight);
        back.size = new Vector2(3, spriteHeight);
    }

    public int CalculateHeight()
    {
        return Mathf.Min(maxHeight, TileMapHelper.AirTileCountAbove(TileMap.Instance, transform.position.ToGridPosition()));
    }

}