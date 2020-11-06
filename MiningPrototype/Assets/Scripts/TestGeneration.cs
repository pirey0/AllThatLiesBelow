using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class TestGeneration : MonoBehaviour
{
    [SerializeField] Tile[] groundTiles;
    [SerializeField] Tilemap tilemap;

    [Header("Settings")]
    [SerializeField] bool updateOnParameterChanged;

    [OnValueChanged("OnParameterChanged")]
    [SerializeField] bool seedIsRandom;

    [OnValueChanged("OnParameterChanged")]
    [SerializeField] int seed;

    [OnValueChanged("OnParameterChanged")]
    [SerializeField] int size;

    [OnValueChanged("OnParameterChanged")]
    [Range(0, 1)]
    [SerializeField] float initialAliveChance;

    [OnValueChanged("OnParameterChanged")]
    [Range(0,9)]
    [SerializeField] int deathLimit;

    [OnValueChanged("OnParameterChanged")]
    [Range(0, 9)]
    [SerializeField] int birthLimit;

    [OnValueChanged("OnParameterChanged")]
    [Range(0,10)]
    [SerializeField] int automataSteps;

    [SerializeField] AnimationCurve heightMultiplyer;


    bool[,] map;


    private void Start()
    {
        RunCompleteProcess();
    }

    [Button]
    private void RunCompleteProcess()
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        Populate();

        IterateX(automataSteps, (x) => RunAutomataStep());

        UpdateVisuals();
        
        stopwatch.Stop();

        Debug.Log("Update Duration: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    private void Populate()
    {
        if (!seedIsRandom)
            Random.InitState(seed);

        map = new bool[size, size];

        IterateXY(size, (x, y) => map[x, y] = heightMultiplyer.Evaluate((float)y/size) * Random.value < initialAliveChance);

    }

    //https://gamedevelopment.tutsplus.com/tutorials/generate-random-cave-levels-using-cellular-automata--gamedev-9664
    private int GetAliveNeightboursCountFor( int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = x + i;
                int neighbour_y = y + j;

                if (i == 0 && j == 0)
                {
                }
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= size || neighbour_y >= size)
                {
                    count = count + 1;
                }
                else if (map[neighbour_x,neighbour_y])
                {
                    count = count + 1;
                }
            }
        }

        return count;
    }

    private bool GetMapAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= size || y >= size)
            return false;

        return map[x, y];
    }

    private void RunAutomataStep()
    {
        IterateXY(size, SingleAutomataSet);
    }

    private void SingleAutomataSet(int x, int y)
    {
        int nbs = GetAliveNeightboursCountFor(x, y);
        map[x, y] = map[x, y] ? nbs > deathLimit : nbs > birthLimit;
    }

    void UpdateVisuals()
    {
        tilemap.ClearAllTiles();
        IterateXY(size, SetTileToMap);
    }

    void OnParameterChanged()
    {
        if (updateOnParameterChanged)
        {
            RunCompleteProcess();
        }
    }

    private void SetTileToMap(int x, int y)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), GetCorrectTile(x,y));
    }

    private Tile GetCorrectTile(int x, int y)
    {
        if (!map[x, y])
            return null;

        int index = GetMapAt(x, y+1) ? 1 : 0;
        index += GetMapAt(x - 1, y) ? 2 : 0;
        index += GetMapAt(x + 1, y) ? 4 : 0;
        index += GetMapAt(x, y - 1) ? 8 : 0;

        return groundTiles[index];
    }

    private void IterateXY(int size, System.Action<int, int> action)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                action(x, y);
            }
        }
    }

    private void IterateX(int size, System.Action<int> action)
    {
        for (int i = 0; i < size; i++)
        {
            action(i);
        }
    }

}
