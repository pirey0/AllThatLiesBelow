using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType
{
    Normal,
    Creative
}

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject normalPlayerPrefab;
    [SerializeField] GameObject creativeModePlayerPrefab;

    [Zenject.Inject] PrefabFactory prefabFactory;

    PlayerType currentType;
    IPlayerController player;
    IPlayerInteraction playerInteraction;
    Transform playerTransform;

    private void Awake()
    {
        var p = prefabFactory.Create(normalPlayerPrefab);
        SetupNewPlayer(p);
    }

    private void SetupNewPlayer(Transform newObject)
    {
        playerTransform = newObject;
        if (newObject != null)
        {
            player = newObject.GetComponent<IPlayerController>();
            playerInteraction = newObject.GetComponent<IPlayerInteraction>();
        }
    }
    public void ChangePlayerTo(PlayerType type)
    {
        if(type != currentType)
        {
            currentType = type;
            Destroy(playerTransform);
            Transform newP = null;
            if (type == PlayerType.Normal)
                newP = prefabFactory.Create(normalPlayerPrefab);
            else if (type == PlayerType.Creative)
                newP = prefabFactory.Create(creativeModePlayerPrefab);
            SetupNewPlayer(newP);
        }
    }
    public IPlayerController GetPlayer()
    {
        return player;
    }

    public Inventory GetPlayerInventory()
    {
        return playerInteraction.Inventory;
    }

    public IPlayerInteraction GetPlayerInteraction()
    {
        return playerInteraction;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public void TeleportPlayerTo(Vector3 newPos)
    {
        player.transform.position = newPos;
    }
}
