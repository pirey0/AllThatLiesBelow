using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogVariableTrigger : MonoBehaviour, INonPersistantSavable
{
    [SerializeField] string variableName;
    [SerializeField] bool setToTrue = true;
    [SerializeField] BoxCollider2D boxCollider;

    [Zenject.Inject] ProgressionHandler progressionHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerController psm))
        {
            progressionHandler.SetVariable(variableName, setToTrue);
            Debug.Log("Trigger: Set" + variableName + " " + setToTrue);
        }
    }

    private void OnValidate()
    {
        name = "DialogVariableTrigger (" + (string.IsNullOrEmpty(variableName) ? "Empty" : variableName + " " + setToTrue) + ")";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
       UnityEditor.Handles.Label(transform.position,  (string.IsNullOrEmpty(variableName) ? "Empty" : variableName + " " + setToTrue));
    }
#endif

    public void Load(SpawnableSaveData data)
    {
        if (data is DialogVariableTriggerSaveData sd)
        {
            variableName = sd.VariableName;
            setToTrue = sd.VariableState;
            boxCollider.offset = sd.colliderOffset.ToVector3();
            boxCollider.size = sd.colliderSize.ToVector3();
            name = "DialogVariableTrigger (" + (string.IsNullOrEmpty(variableName) ? "Empty" : variableName + " " + setToTrue) + ")";
        }
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new DialogVariableTriggerSaveData();
        data.SaveTransform(transform);
        data.VariableName = variableName;
        data.VariableState = setToTrue;
        data.colliderOffset = new SerializedVector3(boxCollider.offset);
        data.colliderSize = new SerializedVector3(boxCollider.size);
        return data;
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID(SpawnableIDType.DialogVariableTrigger);
    }

    [System.Serializable]
    public class DialogVariableTriggerSaveData : SpawnableSaveData
    {
        public string VariableName;
        public bool VariableState;
        public SerializedVector3 colliderOffset;
        public SerializedVector3 colliderSize;
    }
}
