using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

public class SaveHandler : MonoBehaviour
{
    public const string SAVE_NAME = "SaveFile.data";
    public const string SAVEFILE_VERSION = "0.1 Testing";

    public static bool LoadFromSavefile;

    [SerializeField] GameObject[] spawnablePrefabs;
    [Zenject.Inject] PrefabFactory prefabFactory;


    private static string GetFullSavePath()
    {
        return Application.persistentDataPath + SAVE_NAME;
    }

    public void Save()
    {
        BaseSave(GetFullSavePath());
    }

    public void Editor_SaveAs()
    {
        #if UNITY_EDITOR

        string path = UnityEditor.EditorUtility.SaveFilePanel("Save As...", "", "SaveFile", "data");

        if (!string.IsNullOrEmpty(path))
        {
            BaseSave(path);
            UnityEditor.AssetDatabase.Refresh();
        }

        #endif
    }

    private void BaseSave(string path)
    {
        SaveDataCollection collection = new SaveDataCollection();
        collection.Version = SAVEFILE_VERSION;

        var npsm = new NonPersistantSaveManager(); //Non persistant stuff
        collection.Add(npsm.ToSaveData());

        var objects = Util.FindAllThatImplement<ISavable>();
        foreach (var savable in objects)
        {
            collection.Add(savable.ToSaveData());
        }

        var stream = File.Open(path, FileMode.OpenOrCreate);

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, collection);
        Debug.Log("Saved Sucessfully (" + (stream.Length * 0.000001f) + " MB)");
        stream.Close();
    }

    public static bool SaveFileExists()
    {
        return File.Exists(GetFullSavePath());
    }

    public static void DestroySaveFile()
    {
        if (SaveFileExists())
        {
            File.Delete(GetFullSavePath());
        }
    }

    public void Load()
    {
        if (!SaveFileExists())
        {
            Debug.LogError("No Savefile found.");
            return;
        }
        BaseLoad(GetFullSavePath());
        Debug.Log("Loaded Successfully");
    }

    private void BaseLoad(string path)
    {
        var stream = File.Open(path, FileMode.Open);
        BinaryFormatter formatter = new BinaryFormatter();
        SaveDataCollection collection = (SaveDataCollection)formatter.Deserialize(stream);
        stream.Close();

        if (collection.Version != SAVEFILE_VERSION)
        {
            Debug.LogError("Save File is deprecated: Version: " + SAVEFILE_VERSION + " Savefile Version: " + collection.Version);
            return;
        }

        var objects = Util.FindAllThatImplement<ISavable>().OrderBy((x) => x.GetLoadPriority());

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

        //Load non persistant
        var npsm = new NonPersistantSaveManager(); //Non persistant stuff
        npsm.SetSpawnables(spawnablePrefabs, prefabFactory);
        npsm.Load(collection[npsm.GetSaveID()]);
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
