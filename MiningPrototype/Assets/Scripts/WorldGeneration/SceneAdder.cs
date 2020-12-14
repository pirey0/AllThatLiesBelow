using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    int loadingIndex = 0;

    public int LoadingTotal { get => addition.Count; }
    public int LoadingCurrent { get => loadingIndex; }

    public IEnumerator LoadAll()
    {
        return LoadAdditive(addition);
    }

    private IEnumerator LoadAdditive(List<MapAddition> maps)
    {
        loadingIndex = 0;

        int maxTries = 100;
        while (loadingIndex < maps.Count)
        {
            current = maps[loadingIndex];
            while (maxTries-- > 0)
            {
                Vector2Int botLeftCorner = current.CollapseOffset();
                List<Vector2Int> locations = new List<Vector2Int>();
                Util.IterateXY((int)current.Size.x, (int)current.Size.y, (x, y) => locations.Add(botLeftCorner + new Vector2Int(x, y)));

                if (!RuntimeProceduralMap.Instance.IsAdditivelyCoveredAtAny(locations))
                {
                    saveHandler.LoadAdditive(current.SavedSceneFile, current.CollapseOffset().AsV3());
                    Debug.Log("Loaded Scene " + current.SavedSceneFile.name);
                    break;
                }
            }
            if (maxTries == 0)
            {
                Debug.LogError("No space found for " + current.Name);
            }

            yield return null;
            loadingIndex++;
        }
        Debug.Log("Scene Adder finished.");

    }

#if UNITY_EDITOR
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
            UnityEditor.Handles.Label(new Vector3(x - a.Size.x, y), a.Name);
        }
        Gizmos.color = Color.white;
    }
#endif
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