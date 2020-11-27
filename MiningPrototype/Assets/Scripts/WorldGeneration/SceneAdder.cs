using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class SceneAdder : StateListenerBehaviour
{
    [SerializeField] List<MapAddition> addition;

    [Zenject.Inject] DiContainer diContainer;

    bool loaded = false;
    MapAddition current;

    private void Start()
    {
        gameState.ChangeStateTo(GameState.State.Entry);
    }

    protected override void OnStateChanged(GameState.State newState)
    {
        if(newState == GameState.State.PreLoadScenes)
        {
            if (addition.Count > 0)
                StartCoroutine(LoadAdditive());
        }
    }

    private IEnumerator LoadAdditive()
    {
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
        gameState.ChangeStateTo(GameState.State.PostLoadScenes);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (loadMode != LoadSceneMode.Additive)
            return;

        Vector2Int offset = new Vector2Int(Util.RandomInV2(current.XOffsetRange), current.FromTop ? RuntimeProceduralMap.Instance.SizeY - current.YOffset : current.YOffset);
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
}

[System.Serializable]
public struct MapAddition
{
    public Vector2Int XOffsetRange;
    public int YOffset;
    public bool FromTop;
    public SceneReference SceneToAdd;
}