using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour, IDropReceiver
{
    [SerializeField] int burnDelay;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite on, off;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] MonoBehaviour light;
    [SerializeField] AudioSource audioSource;

    private FireState state;
    public FireState State
    {
        get => state;
        set
        {
            state = value;
            if (state == FireState.Off)
            {
                if (burningRoutine != null)
                {
                    StopCoroutine(burningRoutine);
                    burningRoutine = null;
                }
                DisableVisuals();
            } else if (state == FireState.Burning && burningRoutine == null && fuelStored > 0)
            {
                burningRoutine = StartCoroutine(BurningRoutine());
                EnableVisuals();
            }
        }
    }

    private void Start()
    {
        State = FireState.Off;
    }

    private void EnableVisuals()
    {
        spriteRenderer.sprite = on;
        light.enabled = true;
        audioSource.enabled = true;

        var emission = particleSystem.emission;
        emission.enabled = true;
    }

    private void DisableVisuals()
    {
        spriteRenderer.sprite = off;
        light.enabled = false;
        audioSource.enabled = false;

        var emission = particleSystem.emission;
        emission.enabled = false;
    }

    int fuelStored = 0;
    Coroutine burningRoutine;

    IEnumerator BurningRoutine ()
    {
        while (fuelStored > 0)
        {
            fuelStored--;
            yield return new WaitForSeconds(burnDelay);
        }

        State = FireState.Off;
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        bool takes = false;
        var info = ItemsData.GetItemInfo(pair.type);
        if (info != null)
            takes = info.IsBurnable;
        return takes;
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
        if (origin.Contains(pair))
        {
            origin.TryRemove(pair);
            fuelStored += pair.amount;
            State = FireState.Burning;
            particleSystem.Emit(Mathf.Clamp(2*pair.amount,10,100));
        }
    }

    public enum FireState
    {
        Off,
        Burning
    }
}
