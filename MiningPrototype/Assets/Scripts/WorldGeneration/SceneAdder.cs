using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAdder : MonoBehaviour
{
    [SerializeField] Map thisMap;
    [SerializeField] MapAddition addition;


    private void Start()
    {
        if (addition.SceneToAdd != null)
            LoadAddititve();
    }

    private void OnValidate()
    {
        this.name = "SceneAdder_" + addition.SceneToAdd;
    }

    [Button(null, EButtonEnableMode.Playmode)]
    private void LoadAddititve()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(addition.SceneToAdd, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (loadMode != LoadSceneMode.Additive)
            return;

        Debug.Log("Loaded: " + scene.name + " " + scene.rootCount);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Vector2Int offset = new Vector2Int(addition.XOffset, addition.FromTop ? thisMap.SizeY - addition.YOffset : addition.YOffset);

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

    }
}

[System.Serializable]
public struct MapAddition
{
    public int XOffset;
    public int YOffset;
    public bool FromTop;
    public SceneReference SceneToAdd;

}