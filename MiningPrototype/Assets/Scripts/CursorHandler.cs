using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHandler : MonoBehaviour
{
    [SerializeField] Texture2D customCursor, customCursor_pointing, customCursor_crosshair;
    CursorType current;
    CursorType before;
    [SerializeField] bool isHidden = true;

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

    public void Show()
    {
        if (isHidden)
        {
            isHidden = false;
            Cursor.visible = true;
            SetCursor(current);
        }
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
