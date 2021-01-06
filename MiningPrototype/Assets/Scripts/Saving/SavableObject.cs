using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ISavable
{
    SaveData ToSaveData();

    void Load(SaveData data);

    string GetSaveID();

    int GetLoadPriority();
}

public interface INonPersistantSavable
{
    SpawnableSaveData ToSaveData();

    SaveID GetSavaDataID();
    
    void Load(SpawnableSaveData data);

}

[System.Serializable]
public class SaveID
{
    public SpawnableIDType IDType;
    public string IDString;

    public SaveID(SpawnableIDType type)
    {
        IDType = type;
        IDString = "";
    }

    public SaveID(string s)
    {
        IDString = s;
        IDType = SpawnableIDType.None;
    }

    public SaveID(string s, SpawnableIDType type)
    {
        IDString = s;
        IDType = type;
    }

    public string AsStringID()
    {
        if(IDType == SpawnableIDType.None)
        {
            return IDString;
        }
        else
        {
            return IDType.ToString();
        }
    }
}


public enum SpawnableIDType
{
    None,
    Rock,
    Ladder,
    Support,
    Hut,
    LetterBox,
    Altar,
    Crate,
    Ball,
    Chest,
    Torch,
    OverworldLight,
    DropBox,
    Skeleton,
    Hourglass,
    Echo,
    Decoration,
    Platform,
    Mushroom,
    Vine,
    LocationIndicator,
    WoodenBlocking,
    Note,
    Rope,
    Lift,
    Lantern,
    Sign,
    JungleHeart,
    DialogVariableTrigger,
    Vase,
    BigMushroom,
    JungleStatue,
    FleshEatingPlant
}

[System.Serializable]
public class SaveData
{
    public string GUID;
}

[System.Serializable]
public class SpawnableSaveData
{
    public string SpawnableID;
    public SpawnableIDType SpawnableIDType;

    public SerializedVector3 Position;
    public SerializedVector3 Rotation;


    public void SaveTransform(Transform t)
    {
        Position = new SerializedVector3(t.position);
        Rotation = new SerializedVector3(t.eulerAngles);
    }

    public SaveID GetSaveID()
    {
        return new SaveID(SpawnableID, SpawnableIDType);
    }
}