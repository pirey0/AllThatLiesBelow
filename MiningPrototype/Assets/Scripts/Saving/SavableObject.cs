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
    void Load(SpawnableSaveData data);

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
    Lift
}

[System.Serializable]
public class SaveData
{
    public string GUID;
}

[System.Serializable]
public class SpawnableSaveData
{
    public SpawnableIDType SpawnableIDType;
    public SerializedVector3 Position;
    public SerializedVector3 Rotation;


    public void SaveTransform(Transform t)
    {
        Position = new SerializedVector3(t.position);
        Rotation = new SerializedVector3(t.eulerAngles);
    }
}