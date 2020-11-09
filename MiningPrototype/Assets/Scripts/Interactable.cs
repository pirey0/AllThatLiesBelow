using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void BeginInteracting(GameObject interactor);
    void EndInteracting(GameObject interactor);

    GameObject gameObject { get; }
}
