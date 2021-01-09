using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPreview : MonoBehaviour, IItemPreview
{
    bool couldPlace;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Zenject.Inject] RuntimeProceduralMap map;

    public Vector3 GetPlacePosition(Vector3 pointPosition)
    {
        return pointPosition.ToGridPosition().AsV3() + new Vector3(0.5f, 0);
    }

    public void UpdatePreview(Vector3 position)
    {
        var gridPos = position.ToGridPosition();
        transform.position = gridPos.AsV3() + new Vector3(0.5f, 0);

        if (map.IsAirAt(gridPos.x, gridPos.y))
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
