using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : MonoBehaviour
{
    [SerializeField] GameObject outside_foreground, inside_foreground;
    [SerializeField] bool isOpen = false;
    [SerializeField] AudioSource doorAudio;
    [SerializeField] Transform cameraTarget;

    public delegate void HutStateChange(bool isOpen);
    public event HutStateChange OnHutStateChange;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() == null)
            return;

        //enable this section when you want the hut to stay open when the player digs into the ground
        //Debug.LogWarning(collision.transform.position + 0.5f * Vector3.up + " > " + transform.position + ((collision.transform.position.y - 0.5f) > transform.position.y).ToString());
        //if ((collision.transform.position.y+0.5f) > transform.position.y)
            Leave();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() == null)
            return;

        Enter();
    }

    private void Leave()
    {
        if (!isOpen)
            return;

        Toggle();
    }
    private void Enter()
    {
        if (isOpen)
            return;

        Toggle();
    }

    private void Toggle()
    {
        isOpen = !isOpen;
        OnHutStateChange(isOpen);

        outside_foreground.SetActive(!isOpen);
        inside_foreground.SetActive(isOpen);

        doorAudio.pitch = UnityEngine.Random.Range(0.75f,1.5f);
        doorAudio.Play();

        if (isOpen)
            CameraController.Instance.TransitionToNewTarget(cameraTarget);
        else
            CameraController.Instance.TransitionToDefault();
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
