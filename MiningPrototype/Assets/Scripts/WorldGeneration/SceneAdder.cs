using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        Time.timeScale = 0;

        while (i < maps.Count)
        {
            current = maps[i];

            saveHandler.LoadAdditive(current.SavedSceneFile, current.CollapseOffset().AsV3());
            Debug.Log("Loaded Scene " + current.SavedSceneFile.name);

            yield return null;

            i++;
        }
        Debug.Log("Scene Adder finished.");
        Time.timeScale = 1;
        if (transitionState)
            gameState.ChangeStateTo(GameState.State.PostLoadScenes);
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
    public TextAsset SavedSceneFile;
    public string Name
    {
        get => SavedSceneFile.name;
    }

    public Vector2Int CollapseOffset()
    {
        int x = Mathf.FloorToInt(RuntimeProceduralMap.Instance.SizeX * Util.RandomInV2(XOffsetRange));
        int y = Mathf.FloorToInt(RuntimeProceduralMap.Instance.SizeY * YOffset);
        return new Vector2Int(x, y);
    }
}