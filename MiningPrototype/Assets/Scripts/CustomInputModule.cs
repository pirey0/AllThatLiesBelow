using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInputModule : StandaloneInputModule
{
    public PointerEventData GetPointerEventData(int pointerId = -1)
    {
        PointerEventData eventData;
        GetPointerData(pointerId, out eventData, true);
        return eventData;
    }
}
