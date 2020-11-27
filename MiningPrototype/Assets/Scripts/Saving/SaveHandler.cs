using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

public static class SaveHandler
{

    public const string SAVE_NAME = "SaveFile.data";
    public const string SAVEFILE_VERSION = "0.1 Testing";

    public static string FULL_SAVE_PATH = Application.persistentDataPath + SAVE_NAME;

    public static bool LoadFromSavefile;

    public static void Save()
    {
        SaveDataCollection collection = new SaveDataCollection();
        collection.Version = SAVEFILE_VERSION;

        var objects = Util.FindAllThatImplement<ISavable>();

        foreach (var savable in objects)
        {
                collection.Add(savable.ToSaveData());
        }

        var stream = File.Open(FULL_SAVE_PATH, FileMode.OpenOrCreate);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, collection);
        Debug.Log("Saved Sucessfully (" + (stream.Length*0.000001f) + " MB)");
        stream.Close();
    }

    public static bool SaveFileExists()
    {
        return File.Exists(FULL_SAVE_PATH);
    }

    public static void DestroySaveFile()
    {
        if (SaveFileExists())
        {
            File.Delete(FULL_SAVE_PATH);
        }
    }

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

        if (collection.Version != SAVEFILE_VERSION)
        {
            Debug.LogError("Save File is deprecated: Version: " + SAVEFILE_VERSION + " Savefile Version: " + collection.Version);
            return;
        }

        var objects = Util.FindAllThatImplement<ISavable>().OrderBy((x)=> x.GetLoadPriority());

        foreach (var savable in objects)
        {
            if (collection.ContainsKey(savable.GetSaveID()))
            {
                var data = collection[savable.GetSaveID()];
                savable.Load(data);
            }
            else
            {
                Debug.LogError("No save data found for " + savable.GetSaveID());
            }
        }

        Debug.Log("Loaded Successfully");
    }

}

[System.Serializable]
public class SaveDataCollection
{
    public string Version;
    public Dictionary<string, SaveData> SaveDatas = new Dictionary<string, SaveData>();

    public SaveData this[string id] { get => SaveDatas[id]; }

    public bool ContainsKey(string key)
    {
        return SaveDatas.ContainsKey(key);
    }

    public void Add(SaveData data)
    {
        if (string.IsNullOrEmpty(data.GUID))
        {
            Debug.LogError("SaveData " + data.GetType() + " has no GUID assigned.");
        }
        else
        {
            SaveDatas.Add(data.GUID, data);
        }
    }
}
