using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeadbobToLocaloffsetUpdater : MonoBehaviour
{
    [SerializeField] SpriteAnimator toListenTo;
    [SerializeField] Sprite[] bob;
    bool isBobbed = false;

    Vector3 localPosition;
    [SerializeField] Vector3 offsetOnBob;

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
            DoBob(bob.Contains(newSprite));
    }

    private void DoBob(bool bob)
    {
        if (bob == isBobbed)
            return;

        isBobbed = bob;

        transform.localPosition = localPosition + ((isBobbed) ? offsetOnBob:Vector3.zero);
    }
}
