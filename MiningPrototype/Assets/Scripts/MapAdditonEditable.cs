using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapAdditionBase : MonoBehaviour
{
    public Vector2 Size;
    public TextAsset SavedSceneFile;

    [Tooltip("Should the addition block other additions from drawing to the same area?")]
    public bool BlocksArea;
    public abstract Vector2Int GetSpawnLocation(int stepSize);

}

public enum Area
{
    None, Mining, Jungle, Ice, ElDorado, Final
}

[ExecuteInEditMode]
public class MapAdditonEditable : MapAdditionBase
{
    public Color gizmoColor;
    public Area Area;
    public bool SpawnAttached;

    [NaughtyAttributes.OnValueChanged("OnInAreaChange")]
    public bool XInArea;

    [NaughtyAttributes.OnValueChanged("OnInAreaChange")]
    public bool YInArea;

    public Vector3 areaTopRight;
    public Vector3 areaBotLeft;

    private Vector2Int? collapsedLocation;

    public override Vector2Int GetSpawnLocation(int stepSize)
    {
        Vector2Int pos = (transform.position - (Vector3)Size * 0.5f).ToGridPosition();

        if (XInArea)
            pos.x = Mathf.FloorToInt(UnityEngine.Random.Range(areaBotLeft.x, areaTopRight.x) - Size.x * 0.5f);

        if (YInArea)
            pos.y = Mathf.FloorToInt(UnityEngine.Random.Range(areaBotLeft.y, areaTopRight.y) - Size.y * 0.5f);

        pos.x -= pos.x % stepSize;
        pos.y -= pos.y % stepSize;

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

        UnityEditor.Handles.Label(transform.position + new Vector3(-Size.x / 2, -Size.y / 2), name + (Area == Area.None ? "" : "(" + Area.ToString() + ")"));
    }

#endif
}
