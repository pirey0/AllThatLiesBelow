using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorHandler : StateListenerBehaviour
{
    [SerializeField] Texture2D customCursor, customCursor_pointing, customCursor_crosshair;
    CursorType current;
    CursorType before;
    [SerializeField] bool isHidden = true;
    [Zenject.Inject] CustomInputModule customInputModule;

    public void SetCursor(CursorType type)
    {
        before = current;
        current = type;

        if (!isHidden)
            Cursor.SetCursor(GetTextureFromType(current), GetOffsetFromType(current), CursorMode.Auto);
    }

    public void TryUnsetCursor(CursorType type)
    {
        if (current == type)
            SetCursor(before);
    }

    public void Hide ()
    {
        isHidden = true;
        Cursor.visible = false;
    }

    protected override void OnPostLoadFromFile()
    {
        Show();
    }

    protected override void OnNewGame()
    {
        Show();
    }

    public void Show()
    {
        if (isHidden)
        {
            isHidden = false;
            Cursor.visible = true;
            SetCursor(current);
        }
    }

    private void Update()
    {
        GameObject hovered = customInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject;
        if (hovered != null && hovered.GetComponent<Button>() != null)
            SetCursor(CursorType.Interactable);
    }

    private Texture2D GetTextureFromType(CursorType cursorType)
    {
        switch (cursorType)
        {
            case CursorType.Mining:
                return customCursor_crosshair;

            case CursorType.Interactable:
                return customCursor_pointing;
        }

        return customCursor;
    }

    private Vector2 GetOffsetFromType(CursorType cursorType)
    {
        switch (cursorType)
        {
            case CursorType.Mining:
                return new Vector2 (customCursor_crosshair.height / 2, customCursor_crosshair.width / 2);
        }

        return Vector2.zero;
    }
}

public enum CursorType
{
    Default,
    Interactable,
    Mining,
}
