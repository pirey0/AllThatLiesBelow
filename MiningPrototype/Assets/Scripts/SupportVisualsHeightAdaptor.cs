using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportVisualsHeightAdaptor : MonoBehaviour
{
    [SerializeField] SpriteRenderer front, back;

    public void AdaptHeight(int height)
    {
        float spriteHeight = (float)height * (10f / 7.5f);

        front.size = new Vector2(3, spriteHeight);
        back.size = new Vector2(3, spriteHeight);
    }
}
