using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorWorldFollower : MonoBehaviour
{

    protected virtual void OnEnable()
    {
        Map.Instance.MirrorSideChanged += OnMirrorSideChanged;
    }

    protected virtual void OnDisable()
    {
        Map.Instance.MirrorSideChanged -= OnMirrorSideChanged;
    }

    private void OnMirrorSideChanged(Map.MirrorState state)
    {
        int sizeX = Map.Instance.SizeX;

        switch (state)
        {
            case Map.MirrorState.Center:
                if (transform.position.x < 0)
                {
                    transform.position = new Vector3(transform.position.x +sizeX, transform.position.y, transform.position.z);
                }
                else if (transform.position.x >= sizeX)
                {
                    transform.position = new Vector3(transform.position.x - sizeX, transform.position.y, transform.position.z);
                }
                break;

            case Map.MirrorState.Left:
                if (transform.position.x > sizeX / 2)
                {
                    transform.position = new Vector3(transform.position.x - sizeX, transform.position.y, transform.position.z);
                }
                break;

            case Map.MirrorState.Right:
                if (transform.position.x < sizeX / 2)
                {
                    transform.position = new Vector3(transform.position.x + sizeX, transform.position.y, transform.position.z);
                }
                break;
        }

    }
}
