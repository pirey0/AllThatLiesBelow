using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlatformPreview : PlatformBase, IItemPreview
{
    bool couldPlace;

    public Vector3 GetPlacePosition(Vector3 pointPosition)
    {
        return pointPosition.ToGridPosition().AsV3() + new Vector3(0.5f, 0);
    }

    public void UpdatePreview(Vector3 position)
    {
        var gridPos = position.ToGridPosition();
        transform.position = gridPos.AsV3() + new Vector3(0.5f, 0);

        bool tS = RuntimeProceduralMap.Instance.IsAirAt(gridPos.x, gridPos.y);
        bool tL = RuntimeProceduralMap.Instance.IsNeighbourAt(gridPos.x+1, gridPos.y);
        bool tR = RuntimeProceduralMap.Instance.IsNeighbourAt(gridPos.x-1, gridPos.y);

        var p = CalculatePlacement();

        if (tS && (tL || tR) && p.Item2>=minWidth && p.Item2<=maxWidth)
        {
            AdaptPlacementTo(p);
            renderer.color = Color.green;
            couldPlace = true;
        }
        else
        {
            renderer.color = Color.red;
            AdaptPlacementTo((Direction.Right, 2));
            couldPlace = false;
        }
    }

    public bool WouldPlaceSuccessfully()
    {
        return couldPlace;
    }
}
