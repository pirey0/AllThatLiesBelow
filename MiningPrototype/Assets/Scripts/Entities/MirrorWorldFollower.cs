using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorWorldFollower : MonoBehaviour
{
    const int MIRROR_AMOUNT = 20;

    [Zenject.Inject] protected RuntimeProceduralMap map;

    protected virtual void OnEnable()
    {
        map.MirrorSideChanged += OnMirrorSideChanged;
    }

    protected virtual void OnDisable()
    {
        map.MirrorSideChanged -= OnMirrorSideChanged;
    }

    private void OnMirrorSideChanged(RuntimeProceduralMap.MirrorState state)
    {
        int sizeX = Constants.WIDTH;

        switch (state)
        {
            case RuntimeProceduralMap.MirrorState.Center:
                if (transform.position.x < 0)
                {
                    transform.position = new Vector3(transform.position.x +sizeX, transform.position.y, transform.position.z);
                }
                else if (transform.position.x >= sizeX)
                {
                    transform.position = new Vector3(transform.position.x - sizeX, transform.position.y, transform.position.z);
                }
                break;

            case RuntimeProceduralMap.MirrorState.Left:
                if (transform.position.x > sizeX - MIRROR_AMOUNT)
                {
                    transform.position = new Vector3(transform.position.x - sizeX, transform.position.y, transform.position.z);
                }
                break;

            case RuntimeProceduralMap.MirrorState.Right:
                if (transform.position.x < MIRROR_AMOUNT)
                {
                    transform.position = new Vector3(transform.position.x + sizeX, transform.position.y, transform.position.z);
                }
                break;
        }

    }
}
