using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject normalPlayerPrefab;

    [Zenject.Inject] PrefabFactory prefabFactory;

    IPlayerController player;
    IPlayerInteraction playerInteraction;
    Transform playerTransform;

    private void Awake()
    {
        var p = prefabFactory.Create(normalPlayerPrefab);
        playerTransform = p;
        player = p.GetComponent<IPlayerController>();
        playerInteraction = p.GetComponent<IPlayerInteraction>();
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

    internal Transform GetPlayerTransform()
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
