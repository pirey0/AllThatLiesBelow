using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class SceneAdder : StateListenerBehaviour
{
    [SerializeField] List<MapAddition> addition;

    [SerializeField] int width, height;

    [Zenject.Inject] DiContainer diContainer;
    [Zenject.Inject] SaveHandler saveHandler;

    bool loaded = false;
    MapAddition current;


    protected override void OnStateChanged(GameState.State newState)
    {
        if (newState == GameState.State.PreLoadScenes)
        {
            if (addition.Count > 0)
                StartCoroutine(LoadAdditive(addition, transitionState: true));
        }
    }

    private IEnumerator LoadAdditive(List<MapAddition> maps, bool transitionState)
    {
        int i = 0;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Time.timeScale = 0;

        while (i < maps.Count)
        {
            current = maps[i];

            if (current.LoadFileInstadOfScene)
            {
                saveHandler.LoadAdditive(current.SavedSceneFile, current.CollapseOffset().AsV3());
            }
            else
            {
                SceneManager.LoadScene(current.SceneToAdd, LoadSceneMode.Additive);
                loaded = false;
            }

            while (!loaded)
                yield return null;

            i++;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Time.timeScale = 1;
        if (transitionState)
            gameState.ChangeStateTo(GameState.State.PostLoadScenes);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (loadMode != LoadSceneMode.Additive)
            return;

        Vector2Int offset = current.CollapseOffset();
        Debug.Log("Loaded: " + scene.name + " " + scene.rootCount + " at " + offset);

        foreach (var obj in scene.GetRootGameObjects())
        {
            if (obj.TryGetComponent(out EditorMap map))
            {
                RuntimeProceduralMap.Instance.LoadFromMap(map.SaveAsset, offset.x, offset.y);
                DestroyImmediate(obj);
            }
            else
            {
                obj.transform.position += offset.AsV3();
                diContainer.InjectGameObject(obj);
            }
        }
        loaded = true;
    }


    private void LoadSingleAt(SceneReference scene, Vector2Int value)
    {
        MapAddition addition = new MapAddition();
        addition.XOffsetRange = new Vector2((float)value.x / RuntimeProceduralMap.Instance.SizeX, (float)value.x / RuntimeProceduralMap.Instance.SizeX);
        addition.YOffset = (float)value.y / RuntimeProceduralMap.Instance.SizeY;
        addition.SceneToAdd = scene;
        addition.Size = Vector2.zero;
        Debug.Log("Loading " + addition.Name + " at " + value);

        StartCoroutine(LoadAdditive(new List<MapAddition>() { addition }, transitionState: false));
    }


    void OnDrawGizmosSelected()
    {
        foreach (var a in addition)
        {
            float xOffsetDif = (a.XOffsetRange.y - a.XOffsetRange.x) * width;
            float x = xOffsetDif * 0.5f + a.XOffsetRange.x * width + a.Size.x * 0.5f;
            float y = a.YOffset * height + a.Size.y * 0.5f;
            float sizeX = xOffsetDif + a.Size.x;
            float sizeY = a.Size.y;

            Gizmos.color = a.gizmoColor;

            Gizmos.DrawWireCube(new Vector3(x, y, 0), new Vector3(sizeX, 0.5f));
            Gizmos.DrawWireCube(new Vector3(x - a.Size.x * 0.5f, y, 0), new Vector3(a.Size.x, sizeY));
            Handles.Label(new Vector3(x - a.Size.x, y), a.Name);
        }

        Gizmos.color = Color.white;
    }
}

[System.Serializable]
public struct MapAddition
{
    public Vector2 XOffsetRange;
    public float YOffset;
    public Vector2 Size;
    public Color gizmoColor;
    public bool LoadFileInstadOfScene;
    public TextAsset SavedSceneFile;
    public SceneReference SceneToAdd;
    public string Name
    {
        get => FilterNameFromPath();
    }
    private string FilterNameFromPath()
    {
        string path = SceneToAdd.ScenePath;
        List<char> chars = new List<char>();

        bool read = false;
        for (int i = path.Length - 1; i >= 0; i--)
        {
            Char c = path[i];
            if (c == '.')
                read = true;
            else if (c == '/')
                break;
            else if (read)
            {
                chars.Add(c);
                if (Char.IsUpper(c))
                    chars.Add(' ');
            }

        }
        char[] array = chars.ToArray();
        Array.Reverse(array);
        return new string(array);
    }

    public Vector2Int CollapseOffset()
    {
        int x = Mathf.FloorToInt(RuntimeProceduralMap.Instance.SizeX * Util.RandomInV2(XOffsetRange));
        int y = Mathf.FloorToInt(RuntimeProceduralMap.Instance.SizeY * YOffset);
        return new Vector2Int(x, y);
    }
}