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
    private EditorMap currentEditorMap;
    private TextAsset currentSelectedSaveData;
    private int currentSizeX = 100, currentSizeY = 100;

    public static MapToolWindow Instance { get => GetInstance(); }
    public EditorMap EditorMap { get => currentEditorMap; }

    public bool MapSelected()
    {
        return currentEditorMap != null;
    }

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
            window.Show();
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
        if (instance == null)
            instance = this;

        EditorGUILayout.ObjectField(currentEditorMap, typeof(EditorMap), allowSceneObjects: true);
        if (GUILayout.Button("QuickSelect in Scene"))
        {
            currentEditorMap = GameObject.FindObjectOfType<EditorMap>();
            currentSizeX = currentEditorMap.SizeX;
            currentSizeY = currentEditorMap.SizeY;
        }

        if (currentEditorMap == null)
            return;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Selected Tile: " + selectedTileType.ToString());
        DisplayTileTypeOptions();

        EditorGUILayout.Space();


        EditorGUILayout.Space();

        if (currentEditorMap.IsEditorReady())
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
                currentEditorMap.Save();

            if (GUILayout.Button("Save As"))
                currentEditorMap.SaveAs();

            EditorGUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("Nothing to Save");
        }



        if (GUILayout.Button("Load Previous"))
        {
            currentEditorMap.Load();
        }

        if (GUILayout.Button("Load From"))
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Pick Map", "Assets/Others/Maps/", new string[] { "Map", "bytes" });
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(Util.MakePathRelative(path));
            currentEditorMap.Load(asset);

        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Initialize To Size"))
        {
            currentEditorMap.InitializeBlankOfSize(currentSizeX, currentSizeY);
        }

        if (GUILayout.Button("Resize To"))
        {
            currentEditorMap.EditorMapResize(currentSizeX, currentSizeY);
        }

        currentSizeX = EditorGUILayout.IntField(currentSizeX);
        currentSizeY = EditorGUILayout.IntField(currentSizeY);

        EditorGUILayout.EndHorizontal();


        if(GUILayout.Button("Save Scene As"))
        {
            SaveHandler.Editor_SaveAs();
        }
    }

    private void DisplayTileTypeOptions()
    {
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
}
