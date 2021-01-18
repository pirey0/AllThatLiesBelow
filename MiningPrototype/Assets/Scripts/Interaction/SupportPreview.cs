using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemPreview
{
    void UpdatePreview(Vector3 position);
    bool WouldPlaceSuccessfully();
    Vector3 GetPlacePosition(Vector3 pointPosition);
}



public class SupportPreview : SupportBase, IItemPreview
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

        if (map.IsAirAt(gridPos.x, gridPos.y) && map.IsBlockAt(gridPos.x, gridPos.y - 1) && !Util.InOverworld(gridPos.y))
        {
            AdaptHeightTo(CalculateHeight());
            front.color = Color.green;
            front.sortingLayerName = "Support";
            couldPlace = true;
        }
        else
        {
            front.color = Color.red;
            front.sortingLayerName = "PlacingPreview";
            AdaptHeightTo(2);
            couldPlace = false;
        }
    }

    public bool WouldPlaceSuccessfully()
    {
        return couldPlace;
    }
}
