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
    [SerializeField] public Canvas Canvas;

    [Zenject.Inject] PlayerInteractionHandler playerInteractionHandler;
    [Zenject.Inject] CameraController cameraController;


    //protected override void OnEnable()
    //{
    //    RenderPipelineManager.beginCameraRendering += OnCameraRender;
    //    base.OnEnable();
    //}
    //
    //protected override void OnDisable()
    //{
    //    RenderPipelineManager.beginCameraRendering -= OnCameraRender;
    //    base.OnDisable();
    //}
    //
    //private void OnCameraRender(ScriptableRenderContext context, Camera camera)
    //{
    //    if (!Application.isPlaying)
    //        return;
    //
    //    UpdatePosition();
    //    UpdateSprite();
    //}

    private void UpdateSprite()
    {
        (targetGraphic as Image).sprite = playerInteractionHandler.InventoryDisplayState == InventoryState.Open ? open : closed;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        playerInteractionHandler.ToggleInventory();
    }

    //public void UpdatePosition()
    //{
    //    Vector3 worldPoint = cameraController.Camera.ScreenToWorldPoint(new Vector3(Screen.width - Screen.height / 10, Screen.height - Screen.height / 10, -10));
    //    transform.position = new Vector3(worldPoint.x, worldPoint.y);
    //}
}
