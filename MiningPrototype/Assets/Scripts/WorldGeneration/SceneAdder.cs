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
    [SerializeField] int stepSize;
    [SerializeField] bool debug;

    [Zenject.Inject] SaveHandler saveHandler;

    int loadingIndex = 0;
    List<Rect> drawnRects;

    public int LoadingTotal { get => transform.childCount; }
    public int LoadingCurrent { get => loadingIndex; }

    public IEnumerator LoadAll()
    {
        List<MapAdditionBase> addition = new List<MapAdditionBase>();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out MapAdditionBase mab))
            {
                addition.Add(mab);
            }
        }

        return LoadAdditive(addition);
    }

    private IEnumerator LoadAdditive(List<MapAdditionBase> maps)
    {
        MapAdditionBase current;
        loadingIndex = 0;
        List<Vector2Int> locations = new List<Vector2Int>();
        drawnRects = new List<Rect>();

        while (loadingIndex < maps.Count)
        {
            current = maps[loadingIndex];
            int maxTries = 100;
            while (maxTries-- > 0)
            {
                Vector2Int center = current.GetSpawnLocation(stepSize);



                Rect rect = new Rect(center, current.Size);

                //Primary check through Blocking Rects
                if (!Overlap(drawnRects, rect))
                {
                    //Secondary check through set times on map
                    locations.Clear();
                    Util.IterateXY((int)current.Size.x, (int)current.Size.y, (x, y) => locations.Add(center + new Vector2Int(x, y)));
                    if (!RuntimeProceduralMap.Instance.IsAdditivelyCoveredAtAny(locations))
                    {
                        saveHandler.LoadAdditive(current.SavedSceneFile, center.AsV3());
                        Debug.Log("Loaded Scene " + current.SavedSceneFile.name);

                        if (current.BlocksArea)
                        {
                            //add 3 rects to memory, 1 at current location, 1 mirrored left and 1 mirrored right
                            drawnRects.Add(rect);
                            rect.x -= Constants.WIDTH;
                            drawnRects.Add(rect);
                            rect.x += Constants.WIDTH * 2;
                            drawnRects.Add(rect);
                        }
                        break;
                    }
                }
            }
            if (maxTries <= 0)
            {
                Debug.LogError("No space found for " + current.name);
            }

            yield return null;
            loadingIndex++;
        }
        Debug.Log("Scene Adder finished.");
    }


    private bool Overlap(List<Rect> rects, Rect newRect)
    {
        foreach (var r in rects)
        {
            if (r.Overlaps(newRect))
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (debug)
        {
            if (drawnRects != null)
            {
                foreach (var item in drawnRects)
                {
                    Gizmos.DrawCube(item.center, item.size);
                }
            }

            if (stepSize > 4)
            {
                for (int y = 0; y < Constants.HEIGHT; y += stepSize)
                {
                    Gizmos.DrawLine(new Vector3(0, y), new Vector3(Constants.WIDTH, y));
                }
                for (int x = 0; x < Constants.WIDTH; x += stepSize)
                {
                    Gizmos.DrawLine(new Vector3(x, 0), new Vector3(x, Constants.HEIGHT));
                }
            }
        }



 
    }

}
