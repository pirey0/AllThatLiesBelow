using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveHandler
{

    public const string SAVE_NAME = "SaveFile.data";

    public static string FULL_SAVE_PATH = Application.persistentDataPath + SAVE_NAME;

    [Button]
    public static void Save()
    {
        SaveDataCollection collection = new SaveDataCollection();
        collection.Version = Application.version;

        var objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent(out ISavable savable))
            {
                collection.Add(savable.ToSaveData());
            }
        }

        var stream = File.Open(FULL_SAVE_PATH, FileMode.OpenOrCreate);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, collection);
        stream.Close();
        Debug.Log("Saved Sucessfully");
    }

    public static bool SaveFileExists()
    {
        return File.Exists(FULL_SAVE_PATH);
    }

    [Button]
    public static void Load()
    {
        if (!SaveFileExists())
        {
            Debug.LogError("No Savefile found.");
            return;
        }

        var stream = File.Open(FULL_SAVE_PATH, FileMode.Open);

        BinaryFormatter formatter = new BinaryFormatter();
        SaveDataCollection collection = (SaveDataCollection)formatter.Deserialize(stream);
        stream.Close();

        if (collection.Version != Application.version)
        {
            Debug.LogError("Save File is deprecated: Version: " + Application.version + " Savefile Version: " + collection.Version);
            return;
        }

        var objects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent(out ISavable savable))
            {
                if (collection.ContainsKey(savable.GetInstanceID()))
                {
                    var data = collection[savable.GetInstanceID()];
                    savable.Load(data);
                }
                else
                {
                    Debug.LogError("No save data found for " + obj.name + " " + savable.GetInstanceID());
                }
            }
        }

        Debug.Log("Loaded Successfully");
    }
}

[System.Serializable]
public class SaveDataCollection
{
    public string Version;
    public Dictionary<int, SaveData> SaveDatas = new Dictionary<int, SaveData>();

    public SaveData this[int id] { get => SaveDatas[id]; }

    public bool ContainsKey(int key)
    {
        return SaveDatas.ContainsKey(key);
    }

    public void Add(SaveData data)
    {
        SaveDatas.Add(data.InstanceID, data);
    }
}
