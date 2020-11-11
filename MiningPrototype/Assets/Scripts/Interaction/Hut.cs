using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : MonoBehaviour
{
    [SerializeField] GameObject outside_foreground;
    [SerializeField] bool isOpen = false;

    public delegate void HutStateChange(bool isOpen);
    public event HutStateChange OnHutStateChange;

    private void OnTriggerExit2D(Collider2D collision)
    {
        //enable this section when you want the hut to stay open when the player digs into the ground
        //Debug.LogWarning(collision.transform.position + 0.5f * Vector3.up + " > " + transform.position + ((collision.transform.position.y - 0.5f) > transform.position.y).ToString());
        //if ((collision.transform.position.y+0.5f) > transform.position.y)
            Leave();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enter();
    }

    private void Leave()
    {
        if (!isOpen)
            return;

        isOpen = false;
        OnHutStateChange(isOpen);

        outside_foreground.SetActive(true);
    }
    private void Enter()
    {
        if (isOpen)
            return;

        isOpen = true;
        OnHutStateChange(isOpen);

        outside_foreground.SetActive(false);
    }
}
