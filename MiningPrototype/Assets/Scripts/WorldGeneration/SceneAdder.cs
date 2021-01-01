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

    [SerializeField] int width, height;
    [SerializeField] MapAdditonEditable sceneAdderEditablePrefab;

    [Zenject.Inject] DiContainer diContainer;
    [Zenject.Inject] SaveHandler saveHandler;

    MapAdditionBase current;
    int loadingIndex = 0;

    public int LoadingTotal { get => transform.childCount; }
    public int LoadingCurrent { get => loadingIndex; }

    public IEnumerator LoadAll()
    {
        List<MapAdditionBase> addition = new List<MapAdditionBase>();

        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).TryGetComponent(out MapAdditionBase mab))
            {
                addition.Add(mab);
            }
        }

        return LoadAdditive(addition);
    }



    private IEnumerator LoadAdditive(List<MapAdditionBase> maps)
    {
        loadingIndex = 0;

        int maxTries = 100;
        while (loadingIndex < maps.Count)
        {
            current = maps[loadingIndex];
            while (maxTries-- > 0)
            {
                Vector2Int botLeftCorner = current.GetSpawnLocation();
                List<Vector2Int> locations = new List<Vector2Int>();
                Util.IterateXY((int)current.Size.x, (int)current.Size.y, (x, y) => locations.Add(botLeftCorner + new Vector2Int(x, y)));

                if (!RuntimeProceduralMap.Instance.IsAdditivelyCoveredAtAny(locations))
                {
                    saveHandler.LoadAdditive(current.SavedSceneFile, botLeftCorner.AsV3());
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
}
