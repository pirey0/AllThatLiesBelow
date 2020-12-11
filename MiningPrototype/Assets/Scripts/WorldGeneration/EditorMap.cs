using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class EditorMap : RenderedMap
{
    [Header("EditorMap")]
    [SerializeField] TextAsset saveAsset;

    public UnityEngine.TextAsset SaveAsset { get => saveAsset; }

    public override string GetSaveID()
    {
        return "EditorMap";
    }

#if UNITY_EDITOR

    public bool IsEditorReady()
    {
        return !MapIsNull();
    }

    public void InitializeBlankOfSize(int newX, int newY)
    {
        InitMap(newX, newY);
        UpdateAllVisuals();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    public void EditorMapResize(int newX, int newY)
    {
        ResizeMap(newX, newY);
        UpdateAllVisuals();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }

    public void SaveAs()
    {
        saveAsset = null;
        SaveRecent();
        //revert if save failed
    }

    public void SaveRecent()
    {
        if (!IsEditorReady())
        {
            Debug.LogError("Nothing to save.");
        }

        string path;

        if (saveAsset == null)
        {
            path = UnityEditor.EditorUtility.SaveFilePanel("Map", "Assets/Other/Maps", "NewSavedScene", "bytes");
        }
        else
        {
            path = UnityEditor.AssetDatabase.GetAssetPath(saveAsset);
        }

        if (path != null)
        {
            DurationTracker tracker = new DurationTracker("Map saving");

            //SAVE 
            SaveHandler.Editor_SaveAs(path);

            UnityEditor.AssetDatabase.Refresh();
            path = Util.MakePathRelative(path);
            saveAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            UnityEditor.Undo.RecordObject(this, "SaveAsset changed");
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            tracker.Stop();
            Debug.Log("Saved at " + path);
        }
    }

    public void LoadRecent()
    {
        DurationTracker tracker = new DurationTracker("Map Loading");
        if (saveAsset != null)
        {
            var data = SaveHandler.LoadMapOnlyFrom(saveAsset);
            Load(data);

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
        else
            Debug.LogError("No asset to load from.");
        tracker.Stop();
    }

    public void LoadFrom(TextAsset asset)
    {
        saveAsset = asset;
        UnityEditor.Undo.RecordObject(this, "SaveAsset changed");
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        LoadRecent();
    }

    public void LoadFromOldMapFile(TextAsset asset)
    {
        if (saveAsset != null)
        {
            var saveData = MapHelper.LoadMapSaveDataFromTextAsset(asset);
            if (saveData != null)
            {
                Load(saveData);
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }

    public void SaveAsOld()
    {
        string path = UnityEditor.EditorUtility.SaveFilePanel("Map", "Assets/Other/Maps", "NewSavedScene", "bytes");
        DurationTracker tracker = new DurationTracker("Map saving OLD");
        BinaryFormatter formatter = new BinaryFormatter();

        var stream = File.Open(path, FileMode.OpenOrCreate);
        formatter.Serialize(stream, ToSaveData());
        stream.Close();
    }
#endif
}