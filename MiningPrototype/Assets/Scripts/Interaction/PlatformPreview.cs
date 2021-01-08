using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlatformPreview : PlatformBase, IItemPreview
{
    bool couldPlace;

    public Vector3 GetPlacePosition(Vector3 pointPosition)
    {
        return pointPosition.ToGridPosition().AsV3();
    }

    public void UpdatePreview(Vector3 position)
    {
        var gridPos = position.ToGridPosition();
        transform.position = gridPos.AsV3();

        bool tS = RuntimeProceduralMap.Instance.IsAirAt(gridPos.x, gridPos.y);
        bool tL = RuntimeProceduralMap.Instance.IsBlockAt(gridPos.x+1, gridPos.y);
        bool tR = RuntimeProceduralMap.Instance.IsBlockAt(gridPos.x-1, gridPos.y);

        if (tS && (tL || tR))
        {
            renderer.color = Color.green;
            couldPlace = true;
        }
        else
        {
            renderer.color = Color.red;
            couldPlace = false;
        }
    }

    public bool WouldPlaceSuccessfully()
    {
        return couldPlace;
    }
}
