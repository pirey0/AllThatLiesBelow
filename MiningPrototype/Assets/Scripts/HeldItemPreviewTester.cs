using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldItemPreviewTester : MonoBehaviour
{
    [Zenject.Inject] PlayerManager playerManager;

    int i = 0;
    const int MAX_ITEM_ID = 38;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryNext();
        }
    }

    private void TryNext()
    {
        i = (i+1)%MAX_ITEM_ID;

        var info = ItemsData.GetItemInfo((ItemType)i);

        playerManager.GetPlayerInteraction().SetHeldItemSprite(info);
    }
}
