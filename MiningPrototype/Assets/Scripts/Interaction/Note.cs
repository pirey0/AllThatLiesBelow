using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MirrorWorldFollower, IInteractable, INonPersistantSavable
{
    [SerializeField] ItemAmountPair item;

    [Zenject.Inject] ReadableItemHandler readableItemHandler;

    private event System.Action<IInteractable> forceQuit;

    private void Start()
    {

    }

    public void BeginInteracting(GameObject interactor)
    {
        readableItemHandler.HideEvent += OnHide;
        readableItemHandler.Display(item.amount, null);
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
        data.item = item;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        return data;
    }

    public void Load(SpawnableSaveData data)
    {
        if(data is NoteSaveData sdata)
        {
            item = sdata.item;
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
        public ItemAmountPair item;
    }
}
