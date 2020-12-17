using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void BeginInteracting(GameObject interactor);
    void EndInteracting(GameObject interactor);

    void SubscribeToForceQuit(System.Action action);
    void UnsubscribeToForceQuit(System.Action action);

    Vector3 GetPosition();

    GameObject gameObject { get; }
}
