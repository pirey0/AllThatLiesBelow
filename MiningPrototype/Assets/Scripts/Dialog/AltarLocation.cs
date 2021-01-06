﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IDialogUser
{
    void Setup(INodeServiceProvider services, AltarBaseNode node);
}

public class AltarLocation : PlayerTrigger
{

    [SerializeField] bool encounter;
    [NaughtyAttributes.HideIf("encounter")]
    [SerializeField] string dialogName;
    [SerializeField] GameObject dialogVisualizerPrefab;
    [SerializeField] GameObject visualAltarPrefab;
    [SerializeField] GameObject altarInventoryPrefab;

    [SerializeField] Transform visualizerSpawnLocation, visualAltarSpawnLocation, altarInventorySpawnLocation;

    [Zenject.Inject] ProgressionHandler progressionHandler;
    [Zenject.Inject] PrefabFactory prefabFactory;

    BasicDialogServiceProvider serviceProvider;

    Transform visualizer;
    Transform visualEntity;
    Transform inventoryObject;

    public override void OnPlayerEnter()
    {
        if (dialogVisualizerPrefab != null)
        {
            serviceProvider = new BasicDialogServiceProvider(progressionHandler);
            var node = GetFittingAltarNode();

            if (node != null)
            {
                visualizer = prefabFactory.Create(dialogVisualizerPrefab, visualizerSpawnLocation.position, Quaternion.identity, null);
                inventoryObject = prefabFactory.Create(altarInventoryPrefab, altarInventorySpawnLocation.position, Quaternion.identity, null);

                if (inventoryObject != null)
                {
                    inventoryObject.gameObject.SetActive(false);
                    serviceProvider.SetInventory(inventoryObject.gameObject);
                }

                if (visualizer.TryGetComponent(out IDialogVisualizer vis))
                {
                    serviceProvider.SetVisualizer(vis);
                    visualEntity = prefabFactory.Create(visualAltarPrefab, visualAltarSpawnLocation.position, Quaternion.identity, null);

                    if (visualEntity.TryGetComponent(out IDialogUser dialogUser))
                    {
                        dialogUser.Setup(serviceProvider, node);
                    }
                }
            }
            else
            {
                Debug.Log("Altar not spawned because no dialog is available.");
            }
        }
    }

    private AltarBaseNode GetFittingAltarNode()
    {
        AltarBaseNode node = null;
        if (encounter)
        {
            node = progressionHandler.AltarDialogCollection.GetFirstViableEncounter(serviceProvider);

            if (node == null)
                Debug.LogError("No available Encounter");

        }
        else
        {
            node = progressionHandler.AltarDialogCollection.FindDialogWithName(dialogName);
            if (node == null)
                Debug.LogError("Unable to find dialog named " + dialogName);
        }

        return node;
    }

    public override void OnPlayerExit()
    {
        if (visualizer != null)
            Destroy(visualizer.gameObject);
        if (visualEntity != null)
            Destroy(visualEntity.gameObject);
        if (inventoryObject != null)
            Destroy(inventoryObject.gameObject);

        serviceProvider = null;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position, encounter ? "Enconter" : dialogName);

        if (visualAltarSpawnLocation != null)
         UnityEditor.Handles.Label(visualAltarSpawnLocation.position, "A");

        if (visualizerSpawnLocation != null)
            UnityEditor.Handles.Label(visualizerSpawnLocation.position, "V");
        
        if (altarInventorySpawnLocation != null)
            UnityEditor.Handles.Label(altarInventorySpawnLocation.position, "I");
    }
#endif

}