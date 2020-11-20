using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapToolWindow : EditorWindow
{
    private static MapToolWindow instance;
    private TileType selectedTileType;
    private string[] tileTypeNames;
    public static MapToolWindow Instance { get => GetInstance(); }



    public TileType SelectedTile { get => selectedTileType; }

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/MapToolWindow")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MapToolWindow window = (MapToolWindow)EditorWindow.GetWindow(typeof(MapToolWindow));
        window.Show();

    }

    static MapToolWindow GetInstance()
    {
        if (instance == null)
        {
            MapToolWindow window = (MapToolWindow)EditorWindow.GetWindow(typeof(MapToolWindow));
            instance = window;
        }
        return instance;
    }

    private void LoadTileTypeNames()
    {
        tileTypeNames = System.Enum.GetNames(typeof(TileType));
    }

    private void OnDestroy()
    {
        instance = null;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField(Map.Instance == null ? "No Map Selected" : "Selected Map: " + Map.Instance.name);

        if (GUILayout.Button("Selected Map in Scene"))
        {
            var map = GameObject.FindObjectOfType<Map>();
            map.SelectThis();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Selected Tile: " + selectedTileType.ToString());

        if (tileTypeNames == null)
            LoadTileTypeNames();

        GUILayout.BeginHorizontal();
        for (int i = 0; i < tileTypeNames.Length; i++)
        {

            if (GUILayout.Button(tileTypeNames[i]))
            {
                selectedTileType = (TileType)System.Enum.Parse(typeof(TileType), tileTypeNames[i]);
            }

            if (i % 5 == 4)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
        }
        GUILayout.EndHorizontal();

    }

    /*
    private static void Save()
    {
        var sav = ScriptableObject.CreateInstance>();

        foreach (var item in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (item.TryGetComponent(out Map map))
            {
                sav.mapData = map.Data;
                break;
            }
        }
        sav.GameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        string path = EditorUtility.SaveFilePanel("Save Loadable Map", Application.dataPath, "Map", "asset");

        if (path != "")
        {
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            AssetDatabase.CreateAsset(sav, path);
            Debug.Log("Saved at " + path);
        }
    }
    */
}
