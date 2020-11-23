using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamitePreview : MonoBehaviour, IItemPreview
{
    [SerializeField] SpriteRenderer spriteRenderer;
    bool couldPlace;

    public Vector3 GetPlacePosition(Vector3 pointPosition)
    {
        return pointPosition.ToGridPosition().AsV3() + new Vector3(0.5f, 0);
    }
    
    public void UpdatePreview(Vector3 position)
    {
        var gridPos = position.ToGridPosition();
        transform.position = gridPos.AsV3() + new Vector3(0.5f, 0);
    
        if (RuntimeProceduralMap.Instance.IsAirAt(gridPos.x, gridPos.y))
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
