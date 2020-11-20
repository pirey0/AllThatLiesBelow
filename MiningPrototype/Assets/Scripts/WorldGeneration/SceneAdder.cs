using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAdder : MonoBehaviour
{
    [SerializeField] Map thisMap;
    [SerializeField] List<MapAddition> addition;
    bool loaded = false;
    MapAddition current;

    private void Start()
    {
        if (addition.Count > 0)
            StartCoroutine(LoadAdditive());
    }

    private void OnValidate()
    {
        this.name = "SceneAdder";
    }

    private IEnumerator LoadAdditive()
    {
        GameState.Instance.ChangeStateTo(GameState.State.Loading);

        int i = 0;
        SceneManager.sceneLoaded += OnSceneLoaded;

        while (i < addition.Count)
        {
            current = addition[i];
            
            SceneManager.LoadScene(current.SceneToAdd, LoadSceneMode.Additive);
            loaded = false;

            while (!loaded) 
                yield return null;
            
            i++;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameState.Instance.ChangeStateTo(GameState.State.Ready);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (loadMode != LoadSceneMode.Additive)
            return;

        Vector2Int offset = new Vector2Int(Util.RandomInV2(current.XOffsetRange), current.FromTop ? thisMap.SizeY - current.YOffset : current.YOffset);
        Debug.Log("Loaded: " + scene.name + " " + scene.rootCount + " at " + offset);

        foreach (var obj in scene.GetRootGameObjects())
        {
            if (obj.TryGetComponent(out Map map))
            {
                Debug.Log("Adding from data " + map.Data);
                thisMap.LoadFromMap(map.Data, offset.x, offset.y);
                DestroyImmediate(obj);
            }
            else
            {
                obj.transform.position += offset.AsV3();
            }
        }
        loaded = true;
    }
}

[System.Serializable]
public struct MapAddition
{
    public Vector2Int XOffsetRange;
    public int YOffset;
    public bool FromTop;
    public SceneReference SceneToAdd;

}