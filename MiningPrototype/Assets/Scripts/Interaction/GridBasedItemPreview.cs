using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBasedItemPreview : MonoBehaviour, IItemPreview
{
    [SerializeField] Vector3 offset;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] bool underworldOnly = false;
    bool couldPlace;

    public Vector3 GetPlacePosition(Vector3 pointPosition)
    {
        return pointPosition.ToGridPosition().AsV3() + offset;
    }
    
    public void UpdatePreview(Vector3 position)
    {
        var gridPos = position.ToGridPosition();
        transform.position = gridPos.AsV3() + offset;
    
        if (RuntimeProceduralMap.Instance.IsAirAt(gridPos.x, gridPos.y) && (!underworldOnly || gridPos.y < Constants.OVERWORLD_START_Y))
        {
            spriteRenderer.color = Color.green;
            couldPlace = true;
        }
        else
        {
            spriteRenderer.color = Color.red;
            couldPlace = false;
        }
    }
    
    public bool WouldPlaceSuccessfully()
    {
        return couldPlace;
    }
}
