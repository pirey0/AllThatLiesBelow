using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPersistantSaveManager : ISavable
{
    [SerializeField] GameObject[] spawnablePrefabs;

    PrefabFactory prefabFactory;

    public void SetSpawnables(GameObject[] spawnables, PrefabFactory factory)
    {
        spawnablePrefabs = spawnables;
        this.prefabFactory = factory;
    }

    public string GetSaveID()
    {
        return "Non Persistant_Save_Manager";
    }

    public int GetLoadPriority()
    {
        return -5;
    }

    public void Load(SaveData saveData)
    {
        Debug.Log("Loading NonPersistant");

        if(saveData is SpawnableSaveDataContainer container)
        {
            foreach (var data in container)
            {
                if (data.SpawnableIDType == SpawnableIDType.None)
                    continue;

                if((int)data.SpawnableIDType < spawnablePrefabs.Length)
                {
                    var t = prefabFactory.Create(spawnablePrefabs[(int)data.SpawnableIDType]);
                    t.position = data.Position.ToVector3();
                    t.eulerAngles = data.Rotation.ToVector3();
                    var savComp = t.GetComponent<INonPersistantSavable>();
                    savComp.Load(data);
                }
            }

            Debug.Log("Loaded " + container.Count + " non persistant savables.");
        }
    }

    public SaveData ToSaveData()
    {
        Debug.Log("Saving NonPersistant");

        var objs = Util.FindAllThatImplement<INonPersistantSavable>();
        SpawnableSaveDataContainer container = new SpawnableSaveDataContainer();
        container.GUID = GetSaveID();

        foreach (var s in objs)
        {
            container.Add(s.ToSaveData());
        }
        Debug.Log("Saved " + objs.Length + " non persistant savables.");

        return container;
    }
}

[System.Serializable]
public class SpawnableSaveDataContainer : SaveData 
{
    List<SpawnableSaveData> saveDatas;

    public SpawnableSaveDataContainer()
    {
        saveDatas = new List<SpawnableSaveData>();
    }

    public int Count => saveDatas.Count;

    public void Add(SpawnableSaveData saveData)
    {
        saveDatas.Add(saveData);
    }

    public IEnumerator<SpawnableSaveData> GetEnumerator()
    {
        return saveDatas.GetEnumerator();
    }

}