using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeadbobToLocaloffsetUpdater : MonoBehaviour
{
    [SerializeField] SpriteAnimator toListenTo;
    [SerializeField] List<SpriteOffsetPair> sprites;
    [ShowNonSerializedField] bool isBobbed = false;

    Vector3 localPosition;

    private void Start()
    {
        localPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        toListenTo.OnSpriteChange += OnSpriteChange;
    }

    private void OnDisable()
    {
        toListenTo.OnSpriteChange -= OnSpriteChange;
    }

    private void OnSpriteChange(Sprite newSprite)
    {
        if (newSprite == null)
            return;

        foreach (SpriteOffsetPair pair in sprites)
        {
            if (pair.sprite == newSprite)
            {
                DoBob(true, pair.offset);
                return;
            }
        }

        DoBob(false, Vector3.zero);
    }

    private void DoBob(bool bob, Vector3 offsetOnBob)
    {
        if (bob != isBobbed)
            isBobbed = bob;

        transform.localPosition = localPosition + offsetOnBob;
    }
}

[System.Serializable]
public class SpriteOffsetPair
{
    public Sprite sprite;
    public Vector3 offset;
}
