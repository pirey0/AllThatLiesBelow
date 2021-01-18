using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IDialogUser
{
    void Setup(INodeServiceProvider services, AltarBaseNode node);
}

public class AltarLocation : PlayerTrigger, INonPersistantSavable
{

    [SerializeField] bool encounter;
    [NaughtyAttributes.HideIf("encounter")]
    [SerializeField] string dialogName;
    [SerializeField] string locationName;

    [SerializeField] GameObject dialogVisualizerPrefab;
    [SerializeField] GameObject visualAltarPrefab;
    [SerializeField] GameObject altarInventoryPrefab;

    [Zenject.Inject] ProgressionHandler progressionHandler;
    [Zenject.Inject] PrefabFactory prefabFactory;

    BasicDialogServiceProvider serviceProvider;

    Transform visualizer;
    Transform visualEntity;
    Transform inventoryObject;

    public override void OnPlayerEnter()
    {
        if (!string.IsNullOrWhiteSpace(locationName))
        {
            progressionHandler.SetVariable(locationName, true);
        }

        if (dialogVisualizerPrefab != null)
        {
            serviceProvider = new BasicDialogServiceProvider(progressionHandler);
            var node = GetFittingAltarNode();

            if (node != null)
            {
                visualizer = prefabFactory.Create(dialogVisualizerPrefab, transform.position, Quaternion.identity, null);
                inventoryObject = prefabFactory.Create(altarInventoryPrefab, transform.position + Vector3.up, Quaternion.identity, null);

                if (inventoryObject != null)
                {
                    inventoryObject.gameObject.SetActive(false);
                    serviceProvider.SetInventory(inventoryObject.gameObject);
                }


                if (visualizer.TryGetComponent(out IDialogVisualizer vis))
                {
                    serviceProvider.SetVisualizer(vis);
                    visualEntity = prefabFactory.Create(visualAltarPrefab, transform.position, Quaternion.identity, null);

                    var dialogUsers = visualEntity.GetComponents<IDialogUser>();

                    foreach (var user in dialogUsers)
                    {
                        user.Setup(serviceProvider, node);
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
                Debug.Log("No available Encounter");
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
        if (!string.IsNullOrWhiteSpace(locationName))
        {
            progressionHandler.SetVariable(locationName, false);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position, encounter ? "Enconter" : dialogName);
    }
#endif

    public SpawnableSaveData ToSaveData()
    {
        var sd = new AltarLocationSaveData();
        sd.SaveTransform(transform);

        sd.Encounter = encounter;
        sd.DialogName = dialogName;

        return sd;
    }

    public SaveID GetSavaDataID()
    {
        return new SaveID("AltarLocation");
    }

    public void Load(SpawnableSaveData data)
    {
        if (data is AltarLocationSaveData lsd)
        {
            encounter = lsd.Encounter;
            dialogName = lsd.DialogName;
        }
    }


    [System.Serializable]
    public class AltarLocationSaveData : SpawnableSaveData
    {
        public bool Encounter;
        public string DialogName;
    }
}