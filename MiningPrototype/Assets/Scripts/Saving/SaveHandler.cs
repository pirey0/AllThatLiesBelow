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

    [SerializeField] GameObject saveInfoPrefab;
    [SerializeField] GameObject[] spawnablePrefabs;
    [Zenject.Inject] PrefabFactory prefabFactory;


    private static string GetFullSavePath()
    {
        return Application.persistentDataPath + SAVE_NAME;
    }

    public void Save()
    {
        BaseSave(GetFullSavePath());
        if (saveInfoPrefab != null)
            Instantiate(saveInfoPrefab, Vector3.zero, Quaternion.identity);
    }

    public static void Editor_SaveAs(string path)
    {
#if UNITY_EDITOR

        if (string.IsNullOrEmpty(path))
            path = UnityEditor.EditorUtility.SaveFilePanel("Save As...", "", "SavedScene", "bytes");

        if (!string.IsNullOrEmpty(path))
        {
            BaseSave(path);
            UnityEditor.AssetDatabase.Refresh();
        }

#endif
    }

    private static void BaseSave(string path)
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

        var collection = LoadToCollection(GetFullSavePath());
        BaseLoad(collection, Vector3.zero, additiveMode: false);
        Debug.Log("Loaded Successfully");
    }

    public static StatsTracker.StatsTrackerSaveData LoadStatsOnly()
    {
        var collection = LoadToCollection(GetFullSavePath());
        return (StatsTracker.StatsTrackerSaveData)collection.SaveDatas["StatsTracker"];
    }

    private static SaveDataCollection LoadToCollection(string path)
    {
        var stream = File.Open(path, FileMode.Open);
        BinaryFormatter formatter = new BinaryFormatter();
        SaveDataCollection collection = (SaveDataCollection)formatter.Deserialize(stream);
        stream.Close();

        return collection;
    }

    public void LoadAdditive(TextAsset saveAsset, Vector3 offset)
    {
        DurationTracker tracker = new DurationTracker("Map Loading " + saveAsset.name);
        if (saveAsset != null)
        {
            using (var memStream = new MemoryStream())
            {
                var collection = LoadToCollection(saveAsset);
                BaseLoad(collection, offset, additiveMode: true);
            }
        }
        else
        {
            Debug.LogError("No asset to load from.");
        }
        tracker.Stop();
    }

    public static BaseMapSaveData LoadMapOnlyFrom(TextAsset saveAsset)
    {
        var collection = LoadToCollection(saveAsset);

        if (collection.ContainsKey("EditorMap"))
        {
            return (BaseMapSaveData)collection["EditorMap"];
        }
        else
        {
            Debug.LogError("No EditorMap in save.");
            return null;
        }
    }

    private static SaveDataCollection LoadToCollection(TextAsset saveAsset)
    {
        using (var memStream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            memStream.Write(saveAsset.bytes, 0, saveAsset.bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            try
            {
                var data = (SaveDataCollection)formatter.Deserialize(memStream);
                return data;
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                Debug.LogError("Failed to deserialize " + saveAsset.name);
                return null;
            }
        }
    }

    private void BaseLoad(SaveDataCollection collection, Vector3 offset, bool additiveMode)
    {
        if (collection.Version != SAVEFILE_VERSION)
        {
            Debug.LogError("Save File is deprecated: Version: " + SAVEFILE_VERSION + " Savefile Version: " + collection.Version);
            return;
        }

        if (additiveMode)
        {
            if (collection.ContainsKey("EditorMap"))
            {
                var data = collection["EditorMap"];
                RuntimeProceduralMap.Instance.AdditiveLoad((BaseMapSaveData)data, Mathf.FloorToInt(offset.x), Mathf.FloorToInt(offset.y));
            }
        }
        else
        {
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
        }

        //Load non persistant
        var npsm = new NonPersistantSaveManager(); //Non persistant stuff
        npsm.SetSpawnables(spawnablePrefabs, prefabFactory);
        npsm.SetOffset(offset);
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
