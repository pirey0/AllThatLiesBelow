using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour, ISavable
{
    [ReadOnly] [SerializeField] string saveID = Util.GenerateNewSaveGUID();
    [SerializeField] PlayerStateMachine stateMachine;
    [SerializeField] PlayerInteractionHandler interactionHandler;
    public string GetSaveID()
    {
        return saveID;
    }

    public void Load(SaveData data)
    {
        if (data is PlayerSaveData saveData)
        {
            transform.position = saveData.Position.ToVector3();
            interactionHandler.SetInventory(saveData.Inventory);
            stateMachine.ForceToState(saveData.State);
        }
        else
        {
            Debug.LogError("Wrong SaveData for player");
        }
    }

    public SaveData ToSaveData()
    {
        PlayerSaveData saveData = new PlayerSaveData();
        saveData.GUID = saveID;

        saveData.Position = new SerializedVector3(transform.position);
        saveData.State = stateMachine.GetStateMachine().CurrentState.Name;
        saveData.Inventory = interactionHandler.GetInventory();
        return saveData;
    }
}


[System.Serializable]
public class PlayerSaveData : SaveData
{
    public SerializedVector3 Position;
    public Inventory Inventory;
    public string State;
}

[System.Serializable]
public struct SerializedVector3
{
    public float X, Y, Z;

    public SerializedVector3(Vector3 v3)
    {
        X = v3.x;
        Y = v3.y;
        Z = v3.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

}