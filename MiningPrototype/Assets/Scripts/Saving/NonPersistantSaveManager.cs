using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPersistantSaveManager : ISavable
{
    PrefabFactory prefabFactory;
    Vector3 offset;
    Dictionary<string, GameObject> keyPrefabMap;


    public void Setup(PrefabFactory factory)
    {
        this.prefabFactory = factory;

        //LoadSavables
        keyPrefabMap = new Dictionary<string, GameObject>();
        var objects = Resources.LoadAll("SpawnableObjects/");
        foreach (var o in objects)
        {
            if (o is GameObject go)
            {
                if (go.TryGetComponent(out INonPersistantSavable nps))
                {
                   var s = nps.GetSavaDataID().AsStringID();

                    if (string.IsNullOrEmpty(s))
                    {
                        Debug.LogError("Undefined SaveDataID on " + go.name);
                    }
                    else
                    {
                        keyPrefabMap.Add(s, go);
                    }
                }
            }
        }
    }

    public void SetOffset(Vector3 v)
    {
        offset = v;
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
        //Debug.Log("Loading NonPersistant");

        if(saveData is SpawnableSaveDataContainer container)
        {
            foreach (var data in container)
            {
                string saveID = data.GetSaveID().AsStringID();
                if (string.IsNullOrEmpty(saveID))
                {
                    Debug.LogError("Invalid SpawnID on" + data.ToString());
                    continue;
                }

                if (keyPrefabMap.ContainsKey(saveID))
                {
                    var t = prefabFactory.Create(keyPrefabMap[saveID]);
                    t.position = data.Position.ToVector3() + offset;
                    t.eulerAngles = data.Rotation.ToVector3();
                    var savComp = t.GetComponent<INonPersistantSavable>();
                    savComp.Load(data);
                }
                else
                {
                    Debug.LogError("Cannot find prefab for SpawnID " + saveID);
                }
            }

            //Debug.Log("Loaded " + container.Count + " non persistant savables.");
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
            var sd = s.ToSaveData();
            sd.SpawnableIDType = s.GetSavaDataID().IDType;
            sd.SpawnableID = s.GetSavaDataID().IDString;
            container.Add(sd);
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