using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHoverHighligher : HoverHighlighter
{
    [SerializeField] GameObject tutorialUI;
    [SerializeField] InventoryOwner inventoryOwner;
    
    [Zenject.Inject] ProgressionHandler progressionHandler;


    private void Start()
    {
        inventoryOwner = GetComponent<InventoryOwner>();
        inventoryOwner.StateChanged += OnInventoryStateChanged;
    }

    private void OnInventoryStateChanged(InventoryState obj)
    {
        if(obj == InventoryState.Open)
        {
            progressionHandler.NotifyPassedTutorialFor("RightClickToInteract");
            inventoryOwner.StateChanged -= OnInventoryStateChanged;
        }
    }

    public override void HoverEnter(bool isDraggingItem)
    {
        base.HoverEnter(isDraggingItem);


        if (progressionHandler.NeedsTutorialFor("RightClickToInteract"))
        {
            tutorialUI.SetActive(true);
        }
    }

    public override void HoverExit()
    {
        base.HoverExit();
        tutorialUI.SetActive(false);
    }
}
