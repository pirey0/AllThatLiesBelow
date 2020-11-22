using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Map Fill Tool")]
public class MapFillTool : EditorTool
{
    // Serialize this value to set a default value in the Inspector.
    [SerializeField]
    Texture2D m_ToolIcon;

    GUIContent m_IconContent;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "Map Fill Tool",
            tooltip = "Used to fill areas of the Map"
        };

    }

    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }


    public override void OnToolGUI(EditorWindow window)
    {
        Event ev = Event.current;

        Vector3 mousePosition = ev.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        mousePosition = ray.origin;

        Vector3 worldPos = mousePosition.ToGridPosition().AsV3();
        Handles.color = Color.red;
        Handles.DrawLine(worldPos, worldPos + Vector3.right);
        Handles.DrawLine(worldPos, worldPos + Vector3.up);
        Handles.DrawLine(worldPos + Vector3.up, worldPos + new Vector3(1, 1));
        Handles.DrawLine(worldPos + Vector3.right, worldPos + new Vector3(1, 1));

        string message = "No Map Selected";
        if (MapToolWindow.Instance != null)
        {
            var map = MapToolWindow.Instance.EditorMap;
            if (map != null)
            {
                if (map.IsEditorReady())
                {
                    message = map[(int)worldPos.x, (int)worldPos.y].ToString();
                }
                else
                {
                    message = "Map not loaded.";
                }
            }
        }
        Handles.Label(mousePosition, message);

        if (ev.type == EventType.MouseDown)
        {
            if (ev.button != 0)
                return;
            Selection.activeGameObject = null;
            TryDrawToMap(worldPos);
        }

        window.Repaint();
    }

    private static void TryDrawToMap(Vector3 worldPos)
    {
        if (MapToolWindow.Instance == null)
        {
            Debug.Log("No MapToolWindow found.");
        }
        else
        {
            var map = MapToolWindow.Instance.EditorMap;
            if (map == null)
            {
                Debug.Log("No Map Selected.");
            }
            else
            {
                int x = (int)worldPos.x;
                int y = (int)worldPos.y;

                if (map.IsEditorReady())
                {
                    MapHelper.FloodFill(map, map[x, y].Type, MapToolWindow.Instance.SelectedTile, x, y);
                    Debug.Log("Flood Fill at " + x + "/" + y);
                }
                else
                    Debug.LogError("Map is not loaded.");
            }
        }
    }
}