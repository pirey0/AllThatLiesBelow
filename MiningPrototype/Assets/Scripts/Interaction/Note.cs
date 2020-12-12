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
    private event System.Action forceQuit;

    private void Start()
    {
        if (!string.IsNullOrEmpty(text))
            id = readableItemHandler.AddNewReadable(text);
    }

    public void BeginInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent += OnHide;
        readableItemHandler.Display(id, null);
    }

    private void OnHide()
    {
        forceQuit?.Invoke();
    }

    public void EndInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent -= OnHide;
        readableItemHandler.Hide();
        Destroy(gameObject);
    }

    public void SubscribeToForceQuit(Action action)
    {
        forceQuit += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        forceQuit -= action;
    }

    public SpawnableSaveData ToSaveData()
    {
        NoteSaveData data = new NoteSaveData();
        data.Text = text;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.SpawnableIDType = SpawnableIDType.Note;
        return data;
    }

    public void Load(SpawnableSaveData data)
    {
        if(data is NoteSaveData sdata)
        {
            text = sdata.Text;
            id = readableItemHandler.AddNewReadable(text);
        }
    }

    [System.Serializable]
    public class NoteSaveData: SpawnableSaveData
    {
        public string Text;
    }
}
