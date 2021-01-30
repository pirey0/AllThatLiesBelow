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
    [SerializeField] public Transform ShortcutParent;
    [SerializeField] Material bagMaterial;

    [Zenject.Inject] PlayerManager playerManager;
    [Zenject.Inject] CameraController cameraController;

    private bool isBlinking;

    protected override void Start()
    {
        bagMaterial = new Material(targetGraphic.material);
        targetGraphic.material = bagMaterial;
        Canvas.worldCamera = cameraController.Camera;
        playerManager.GetPlayerInteraction().StateChanged += UpdateSprite;
    }

    protected override void OnDestroy()
    {
        if(playerManager != null)
        {
            playerManager.GetPlayerInteraction().StateChanged -= UpdateSprite;
        }
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
        isBlinking = true;
    }

    public void StopBlinking()
    {
        StopAllCoroutines();
        bagMaterial.SetColor("_OverlayColor", new Color(0, 0, 0, 0));
        isBlinking = false;
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

        if (isBlinking && inventoryState == InventoryState.Open)
            StopBlinking();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        bagMaterial.color = Color.white;
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        bagMaterial.color = new Color(0, 0, 0, 0);
        base.OnPointerExit(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        playerManager.GetPlayerInteraction().ToggleInventory();
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return playerManager.GetPlayerInteraction().WouldTakeDrop(pair);
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
        playerManager.GetPlayerInteraction().ReceiveDrop(pair, origin);
    }

    public bool IsSameInventory(Inventory inventory)
    {
        return playerManager.GetPlayerInventory() == inventory;
    }

    //public void UpdatePosition()
    //{
    //    Vector3 worldPoint = cameraController.Camera.ScreenToWorldPoint(new Vector3(Screen.width - Screen.height / 10, Screen.height - Screen.height / 10, -10));
    //    transform.position = new Vector3(worldPoint.x, worldPoint.y);
    //}
}
