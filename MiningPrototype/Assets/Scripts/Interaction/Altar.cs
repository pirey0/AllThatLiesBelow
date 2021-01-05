using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IDropReceiver
{
    bool WouldTakeDrop(ItemAmountPair pair);
    void BeginHoverWith(ItemAmountPair pair);
    void EndHover();
    void HoverUpdate(ItemAmountPair pair);
    void ReceiveDrop(ItemAmountPair pair, Inventory origin);
}

public enum AltarState
{
    Off,
    InProgress
}

public class Altar : StateListenerBehaviour, IInteractable
{
    [SerializeField] Transform cameraTarget;
    [SerializeField] AltarDialogVisualizer visualizer;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] sprites;
    [SerializeField] AudioSource voicesAudio;

    [SerializeField] bool encounter;
    [NaughtyAttributes.HideIf("encounter")]
    [SerializeField] string dialogName;

    [SerializeField] InventoryOwner dialogInventory;

    [Inject] ProgressionHandler progressionHandler;
    [Inject] CameraController cameraController;

    AltarState currentState = AltarState.Off;
    INodeServiceProvider dialogServices;
    Coroutine dialogRoutine;

    private event System.Action NotifyForcedEnd;

    private bool InInteraction { get => currentState != AltarState.Off; }

    protected override void OnRealStart()
    {
        GetComponent<AudioSource>().Play();
        voicesAudio.Play();

        dialogServices = new BasicDialogServiceProvider(progressionHandler.AltarDialogCollection, visualizer, progressionHandler, dialogInventory == null ? null : dialogInventory.Inventory);
    }

    public void BeginInteracting(GameObject interactor)
    {
        dialogServices.Aborted = false;
        Debug.Log("Begin Altar Interaction");
        gameObject.layer = 12;

        if (encounter)
        {
            var dialogNode = progressionHandler.AltarDialogCollection.GetFirstViableEncounter(dialogServices);

            if (dialogNode != null)
            {
                dialogRoutine = StartCoroutine(AltarDialogRunner.RunDialogCoroutine(dialogServices, dialogNode));
            }
            else
            {
                Debug.LogError("No available Encounter");
            }
        }
        else
        {
            var dialogNode = progressionHandler.AltarDialogCollection.FindDialogWithName(dialogName);
            if (dialogNode != null)
            {
                dialogRoutine = StartCoroutine(AltarDialogRunner.RunDialogCoroutine(dialogServices, dialogNode));
            }
            else
            {
                Debug.LogError("Unable to find dialog named " + dialogName);
            }
        }
    }

    public void EndInteracting(GameObject interactor)
    {
        Debug.Log("End Altar Interaction");
        cameraController.TransitionToDefault();
        gameObject.layer = 0;
        dialogServices.Aborted = true;
        
    }

    public Vector3 GetPosition()
    {
        return transform.position + Vector3.up * 3;
    }

    public void SubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd -= action;
    }

    public void EndHover()
    {
        if (this == null)
            return;

        //spriteRenderer.material = spriteDefault;
        //spriteRenderer.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerInteractionHandler>(out var pi))
        {
            pi.ForceInteractionWith(this);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position, encounter ? "Encounter" : dialogName);
    }
#endif
}
