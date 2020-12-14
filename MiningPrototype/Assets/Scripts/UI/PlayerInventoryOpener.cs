using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;
using UnityEngine.EventSystems;

public class PlayerInventoryOpener : Button, IDropReceiver
{
    [SerializeField] Sprite closed, open;
    [SerializeField] public Canvas Canvas;
    [SerializeField] Material bagMaterial;

    [Zenject.Inject] PlayerInteractionHandler playerInteractionHandler;
    [Zenject.Inject] CameraController cameraController;

    protected override void Start()
    {
        bagMaterial = new Material(targetGraphic.material);
        targetGraphic.material = bagMaterial;
        Canvas.worldCamera = cameraController.Camera;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        playerInteractionHandler.StateChanged += UpdateSprite;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        playerInteractionHandler.StateChanged -= UpdateSprite;
    }

    public void Hide()
    {
        targetGraphic.enabled = false;
    }

    public void Show()
    {
        targetGraphic.enabled = true;
    }

    public void StartBlinking()
    {
        StopBlinking();
        StartCoroutine(BliningRoutine());
    }

    public void StopBlinking()
    {
        StopAllCoroutines();
        bagMaterial.SetColor("_OverlayColor", new Color(0, 0, 0, 0));
    }

    IEnumerator BliningRoutine()
    {
        bool blink = true;

        while (true)
        {
            bagMaterial.SetColor("_OverlayColor", blink ? Color.white : new Color(0, 0, 0, 0));
            blink = !blink;
            yield return new WaitForSeconds(0.25f);
        }
    }

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

    private void UpdateSprite(InventoryState inventoryState)
    {
        (targetGraphic as Image).sprite = inventoryState == InventoryState.Open ? open : closed;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        bagMaterial.color = Color.white;
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        bagMaterial.color = new Color(0,0,0,0);
        base.OnPointerExit(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        StopBlinking();
        playerInteractionHandler.ToggleInventory();
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return playerInteractionHandler.WouldTakeDrop(pair);
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        //
    }

    public void EndHover()
    {
        //
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair, Inventory origin)
    {
        playerInteractionHandler.ReceiveDrop(pair, origin);
    }

    //public void UpdatePosition()
    //{
    //    Vector3 worldPoint = cameraController.Camera.ScreenToWorldPoint(new Vector3(Screen.width - Screen.height / 10, Screen.height - Screen.height / 10, -10));
    //    transform.position = new Vector3(worldPoint.x, worldPoint.y);
    //}
}
