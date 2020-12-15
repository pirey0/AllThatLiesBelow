using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapAdditonEditable : MonoBehaviour
{
    SceneAdder sceneAdder;
    MapAddition addition;
    public string sceneName;
    public MapAddition Addition
    {
        get => addition;
    }

    Vector2 mapSize;
    int index;
    internal void Init(MapAddition addition, Vector2 mapSize, SceneAdder sceneAdder, int index)
    {
        name = addition.Name;
        sceneName = addition.Name;
        this.addition = addition;
        this.mapSize = mapSize;
        this.sceneAdder = sceneAdder;
        this.index = index;

        transform.position = new Vector3(((addition.XOffsetRange.x + addition.XOffsetRange.y) / 2) * mapSize.x + addition.Size.x / 2, addition.YOffset * mapSize.y + addition.Size.y/2);
        transform.localScale = new Vector3((addition.XOffsetRange.y - addition.XOffsetRange.x) * mapSize.x + addition.Size.x, addition.Size.y);
    }


    void Update()
    {
        if (transform.hasChanged)
        {
            if (sceneAdder == null)
            {
                sceneAdder = GetComponentInParent<SceneAdder>();
                index = -1;
            }

            Debug.Log("has changed");
            addition.YOffset = (transform.position.y - (addition.Size.y / 2)) / mapSize.y;
            addition.XOffsetRange = CalculateOffset();
            sceneAdder.ModifiedAddition(this,index);
        }
    }

    private Vector2 CalculateOffset()
    {
        float x1 = (transform.position.x - transform.localScale.x / 2) / mapSize.x;
        float x2 = (transform.position.x + transform.localScale.x / 2 - addition.Size.x) / mapSize.x;
        return new Vector2(x1, x2);
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = addition.gizmoColor;
        Gizmos.DrawWireCube(transform.position - Vector3.right * addition.Size.x / 2, addition.Size);
        Gizmos.DrawWireCube(transform.position, new Vector2((addition.XOffsetRange.y-addition.XOffsetRange.x) *mapSize.x + addition.Size.x, 0));
        UnityEditor.Handles.Label(transform.position, name);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position - Vector3.right * addition.Size.x / 2, addition.Size + Vector2.one);
    }
#endif
}
