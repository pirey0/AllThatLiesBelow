using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorWorldFollower : MonoBehaviour
{

    protected virtual void OnEnable()
    {
        TileMap.Instance.MirrorSideChanged += OnMirrorSideChanged;
    }

    protected virtual void OnDisable()
    {
        TileMap.Instance.MirrorSideChanged -= OnMirrorSideChanged;
    }

    private void OnMirrorSideChanged(TileMap.MirrorState state)
    {
        int sizeX = TileMap.Instance.SizeX;

        switch (state)
        {
            case TileMap.MirrorState.Center:
                if (transform.position.x < 0)
                {
                    transform.position = new Vector3(transform.position.x +sizeX, transform.position.y, transform.position.z);
                }
                else if (transform.position.x >= sizeX)
                {
                    transform.position = new Vector3(transform.position.x - sizeX, transform.position.y, transform.position.z);
                }
                break;

            case TileMap.MirrorState.Left:
                if (transform.position.x > sizeX / 2)
                {
                    transform.position = new Vector3(transform.position.x - sizeX, transform.position.y, transform.position.z);
                }
                break;

            case TileMap.MirrorState.Right:
                if (transform.position.x < sizeX / 2)
                {
                    transform.position = new Vector3(transform.position.x + sizeX, transform.position.y, transform.position.z);
                }
                break;
        }

    }
}
