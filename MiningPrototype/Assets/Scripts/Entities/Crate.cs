using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MineableObject, INonPersistantSavable
{
    [SerializeField] SpriteRenderer spriteRenderer;

    [SerializeField] CrateType crateType;

    [SerializeField] Sprite[] crateSprites;

    [SerializeField] float speedToLandSound;
    [SerializeField] AudioSource landSound;

    private void Start()
    {
        SetupCrate();
    }

    public void SetCrateType(CrateType newType)
    {
        crateType = newType;
    }

    private void SetupCrate()
    {
        var rot = transform.rotation;
        transform.rotation = Quaternion.identity;
        spriteRenderer.sprite = crateSprites[(int)crateType];
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = spriteRenderer.bounds.size;
        boxCollider.offset = (spriteRenderer.bounds.center - transform.position);
        transform.rotation = rot;
    }

    public void Pack(ItemAmountPair toPack)
    {
        contains = toPack;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float speed = Mathf.Abs(collision.relativeVelocity.y);
        if (speed > speedToLandSound)
        {
            if (!landSound.isPlaying)
                landSound.Play();
        }
    }

    public override Vector2 GetPosition()
    {
        if (overlayAnimator != null)
            return overlayAnimator.transform.position;
        else
            return transform.position + (spriteRenderer.size.y / 2) * transform.up;
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new CrateSaveData();
        data.SpawnableIDType = SpawnableIDType.Crate;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);
        data.Type = crateType;
        data.Content = contains;
        return data;
    }

    public void Load(SpawnableSaveData dataOr)
    {
        if (dataOr is CrateSaveData data)
        {
            crateType = data.Type;
            contains = data.Content;
            SetupCrate();
        }
    }

    [System.Serializable]
    public class CrateSaveData : SpawnableSaveData
    {
        public ItemAmountPair Content;
        public CrateType Type;
    }
}


public enum CrateType
{
    Mini,
    Small,
    Tall,
    Wider,
    Higher
}