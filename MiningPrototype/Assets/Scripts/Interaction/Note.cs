using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MirrorWorldFollower, IInteractable, INonPersistantSavable
{
    [TextArea(5, 10)]
    [SerializeField] string text;

    [Zenject.Inject] ReadableItemHandler readableItemHandler;

    int id;
    private event System.Action<IInteractable> forceQuit;

    private void Start()
    {
        if (!string.IsNullOrEmpty(text))
            id = readableItemHandler.AddNewReceivedLetter(text);
    }

    public void BeginInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent += OnHide;
        readableItemHandler.Display(id, null);
    }

    private void OnHide()
    {
        forceQuit?.Invoke(this);
    }

    public void EndInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent -= OnHide;
        readableItemHandler.Hide();
        Destroy(gameObject);
    }

    public void SubscribeToForceQuit(Action<IInteractable> action)
    {
        forceQuit += action;
    }

    public void UnsubscribeToForceQuit(Action<IInteractable> action)
    {
        forceQuit -= action;
    }

    public SpawnableSaveData ToSaveData()
    {
        NoteSaveData data = new NoteSaveData();
        data.Text = text;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        return data;
    }

    public void Load(SpawnableSaveData data)
    {
        if(data is NoteSaveData sdata)
        {
            text = sdata.Text;
            id = readableItemHandler.AddNewReceivedLetter(text);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID(SpawnableIDType.Note);
    }

    [System.Serializable]
    public class NoteSaveData: SpawnableSaveData
    {
        public string Text;
    }
}
