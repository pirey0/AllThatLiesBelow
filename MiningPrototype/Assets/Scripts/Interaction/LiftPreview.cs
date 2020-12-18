using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftPreview : MonoBehaviour, IItemPreview
{
    SpriteRenderer[] renderers;
    bool couldPlace;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }
    public Vector3 GetPlacePosition(Vector3 pointPosition)
    {
        return pointPosition.ToGridPosition().AsV3();
    }

    public void UpdatePreview(Vector3 position)
    {
        transform.position = GetPlacePosition(position);
        var gridPos = position.ToGridPosition();

        bool occupied = true;
        bool free = true;
        for (int x = -1; x <= 2; x++)
        {
            Util.DebugDrawTile(new Vector2Int(gridPos.x + x, gridPos.y), Color.black, 0.05f);

            if (RuntimeProceduralMap.Instance.IsAirAt(gridPos.x + x, gridPos.y))
                occupied = false;

            for (int y = -1; y >= -3; y--)
            {
                Util.DebugDrawTile(new Vector2Int(gridPos.x + x, gridPos.y - y), Color.white, 0.05f);
                if (RuntimeProceduralMap.Instance.IsBlockAt(gridPos.x + x, gridPos.y - y))
                    free = false;
            }
        }

        if (free && occupied)
        {
            foreach (var renderer in renderers)
                renderer.color = Color.green;

            couldPlace = true;
        }
        else
        {
            Debug.Log("Free:" + free + " Occupied: " + occupied);
            foreach (var renderer in renderers)
                renderer.color = Color.red;

            couldPlace = false;
        }
    }

    public bool WouldPlaceSuccessfully()
    {
        return couldPlace;
    }
}
