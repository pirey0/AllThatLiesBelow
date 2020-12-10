using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;
using UnityEngine.EventSystems;

public class PlayerInventoryOpener : Button
{
    [SerializeField] Sprite closed, open;

    Camera main;
    Camera Main
    {
        get
        {
            if (main == null)
                main = Camera.main;

            return main;
        }
    }

    PlayerInteractionHandler playerInteractionHandler;
    PlayerInteractionHandler PlayerInteractionHandler
    {
        get
        {
            if (playerInteractionHandler == null)
                playerInteractionHandler = FindObjectOfType<PlayerInteractionHandler>();

            return playerInteractionHandler;
        }
    }

    protected override void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnCameraRender;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnCameraRender;
        base.OnDisable();
    }

    private void OnCameraRender(ScriptableRenderContext context, Camera camera)
    {
        UpdatePosition();
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        (targetGraphic as Image).sprite = PlayerInteractionHandler.InventoryDisplayState == InventoryState.Open ? open : closed;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        PlayerInteractionHandler.ToggleInventory();
    }

    public void UpdatePosition()
    {
        if (Main == null)
            return;

        Vector3 worldPoint = Main.ScreenToWorldPoint(new Vector3(Screen.width - Screen.height / 10, Screen.height - Screen.height / 10, -10));
        transform.position = new Vector3(worldPoint.x, worldPoint.y);
    }
}
