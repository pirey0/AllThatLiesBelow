using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapAdditionBase : MonoBehaviour
{
    public Vector2 Size;
    public Color gizmoColor;
    public TextAsset SavedSceneFile;
    public string Name
    {
        get => SavedSceneFile.name;
    }
    public abstract Vector2Int GetSpawnLocation();
}


[ExecuteInEditMode]
public class MapAdditonEditable : MapAdditionBase
{
    [Header("Settings")]
    public string Category;
    public bool SpawnAttached;

    [NaughtyAttributes.OnValueChanged("OnInAreaChange")]
    public bool XInArea;

    [NaughtyAttributes.OnValueChanged("OnInAreaChange")]
    public bool YInArea;

    public Vector3 areaTopRight;
    public Vector3 areaBotLeft;

    private Vector2Int? collapsedLocation;

    public override Vector2Int GetSpawnLocation()
    {
        Vector2Int pos = (transform.position - (Vector3)Size * 0.5f).ToGridPosition();

        if (XInArea)
            pos.x = Mathf.FloorToInt(UnityEngine.Random.Range(areaBotLeft.x, areaTopRight.x) - Size.x * 0.5f);

        if (YInArea)
            pos.y = Mathf.FloorToInt(UnityEngine.Random.Range(areaBotLeft.y, areaTopRight.y) - Size.y * 0.5f);

        collapsedLocation = pos;
        return pos;
    }

#if UNITY_EDITOR

    private void OnInAreaChange()
    {
        UpdateTopRight(areaTopRight);
        UpdateBotLeft(areaBotLeft);
    }

    public void UpdateTopRight(Vector3 newPos)
    {
        areaTopRight = NormalizeToArea(newPos);
    }

    public void UpdateBotLeft(Vector3 newPos)
    {
        areaBotLeft = NormalizeToArea(newPos);
    }

    private Vector3 NormalizeToArea(Vector3 newPos)
    {
        newPos -= transform.position;

        if (!XInArea)
            newPos.x = 0;

        if (!YInArea)
            newPos.y = 0;

        return transform.position + newPos;
    }

    [NaughtyAttributes.Button]
    public void ResetArea()
    {
        UpdateTopRight(transform.position + transform.localScale * 0.5f);
        UpdateBotLeft(transform.position + transform.localScale * -0.5f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        if (collapsedLocation.HasValue)
            Gizmos.DrawWireCube((collapsedLocation.Value).AsV3() + (Vector3)Size * 0.5f, Size);
        else
            Gizmos.DrawWireCube(transform.position, Size);

        if (XInArea || YInArea)
        {
            Vector3 extend = (areaTopRight - areaBotLeft);
            Gizmos.DrawWireCube(areaBotLeft + extend * 0.5f, extend);
        }
        UnityEditor.Handles.Label(transform.position, name);
    }

#endif
}
