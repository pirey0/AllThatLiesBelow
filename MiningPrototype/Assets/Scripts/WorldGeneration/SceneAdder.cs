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
    [SerializeField] SceneReference altarScene;
    [SerializeField] int width, height;

    [Zenject.Inject] DiContainer diContainer;


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

            SceneManager.LoadScene(current.SceneToAdd, LoadSceneMode.Additive);
            loaded = false;

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

        int x = Mathf.FloorToInt(RuntimeProceduralMap.Instance.SizeX * Util.RandomInV2(current.XOffsetRange));
        int y = Mathf.FloorToInt(RuntimeProceduralMap.Instance.SizeY * current.YOffset);

        Vector2Int offset = new Vector2Int(x, y);
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

    public void LoadAltarAt(Vector2Int value)
    {
        Debug.Log("Loading altar at " + value);
        MapAddition altarAddition = new MapAddition();
        altarAddition.XOffsetRange = new Vector2((float) value.x / RuntimeProceduralMap.Instance.SizeX, (float)value.x / RuntimeProceduralMap.Instance.SizeX);
        altarAddition.YOffset = (float)value.y / RuntimeProceduralMap.Instance.SizeY;
        altarAddition.SceneToAdd = altarScene;
        altarAddition.Size = Vector2.zero;

        StartCoroutine(LoadAdditive(new List<MapAddition>() { altarAddition }, transitionState: false));
    }


    void OnDrawGizmosSelected()
    {
        foreach (var a in addition)
        {
            float xOffsetDif = (a.XOffsetRange.y - a.XOffsetRange.x) * width; 
            float x = xOffsetDif * 0.5f + a.XOffsetRange.x* width + a.Size.x * 0.5f;
            float y = a.YOffset*height + a.Size.y * 0.5f;
            float sizeX = xOffsetDif + a.Size.x;
            float sizeY = a.Size.y;

            Gizmos.color = a.gizmoColor;

            Gizmos.DrawWireCube(new Vector3(x, y, 0), new Vector3(sizeX, 0.5f));
            Gizmos.DrawWireCube(new Vector3(x - a.Size.x * 0.5f, y, 0), new Vector3(a.Size.x, sizeY));
            Handles.Label(new Vector3(x - a.Size.x,y),a.Name);
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
    public SceneReference SceneToAdd;

    string name;
    public string Name
    {
        get
        {
            if (name == "")
                name = FilterNameFromPath();

            return name;
        }

    }
    private string FilterNameFromPath()
    {
        string path = SceneToAdd.ScenePath;
        List<char> chars = new List<char>();

        bool read = false;
        for (int i = path.Length -1; i >= 0; i--)
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
}