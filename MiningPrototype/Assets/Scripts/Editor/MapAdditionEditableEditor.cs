using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MapAdditonEditable))]
public class MapAdditionEditableEditor : NaughtyAttributes.Editor.NaughtyInspector
{
    protected virtual void OnSceneGUI()
    {
        MapAdditonEditable example = (MapAdditonEditable)target;

        Vector3 snap = Vector3.one * 0.5f;
        Handles.DrawWireCube(example.transform.position, example.Size + Vector2.one);

        EditorGUI.BeginChangeCheck();
        Handles.color = Color.green;
        Vector3 topRight = Handles.FreeMoveHandle(example.areaTopRight, Quaternion.identity, 2, snap, Handles.RectangleHandleCap);
        Handles.color = Color.red;
        Vector3 botLeft = Handles.FreeMoveHandle(example.areaBotLeft, Quaternion.identity, 2, snap, Handles.RectangleHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(example, "Changed Size of MapAdditionEditable");

            example.UpdateTopRight(topRight);
            example.UpdateBotLeft(botLeft);
        }
    }

}