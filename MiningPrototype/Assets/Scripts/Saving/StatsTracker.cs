using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsTracker : StateListenerBehaviour, ISavable
{

    [Zenject.Inject] RuntimeProceduralMap map;


    StatsTrackerSaveData data;
    private float timeStamp;

    protected override void OnNewGame()
    {
        data = new StatsTrackerSaveData();
    }

    protected override void OnRealStart()
    {
        timeStamp = Time.time;

        map.MinedBlock += OnBlockMined;
    }

    private void OnBlockMined(TileType obj)
    {
        Debug.Log("Block Mined");
        data.BlocksMined[obj] += 1;
        data.TotalBlocksMined++;
    }

    public void SaveTime()
    {
        data.SecondsPlayed += Time.time - timeStamp;
        timeStamp = Time.time;
    }

    public void LogToConsole()
    {
        SaveTime();

        Debug.Log("-- Stats summary: --  ");
        Debug.Log("PlayTime: " + data.SecondsPlayed);
        Debug.Log("BlocksMined: " + data.TotalBlocksMined);

        foreach (var item in data.BlocksMined)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }

        Debug.Log("----------------------");
    }

    public int GetLoadPriority()
    {
        return 0;
    }

    public string GetSaveID()
    {
        return "StatsTracker";
    }

    public void Load(SaveData data)
    {
        if (data is StatsTrackerSaveData sData)
        {
            this.data = sData;
        }
    }

    public SaveData ToSaveData()
    {
        data.GUID = GetSaveID();
        SaveTime();
        return data;
    }

    [System.Serializable]
    public class StatsTrackerSaveData : SaveData
    {
        public float SecondsPlayed;

        public int TotalBlocksMined;
        public Dictionary<TileType, int> BlocksMined;

        public StatsTrackerSaveData()
        {
            BlocksMined = new Dictionary<TileType, int>();
            foreach (var t in Enum.GetValues(typeof(TileType)))
            {
                BlocksMined.Add((TileType)t, 0);
            }
        }
    }


}
