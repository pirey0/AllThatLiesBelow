using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : StateListenerBehaviour
{
    [SerializeField] GameObject outside_foreground, inside_foreground;
    [SerializeField] bool isOpen = false;
    [SerializeField] AudioSource doorAudio;
    [SerializeField] Transform cameraTarget;

    [Zenject.Inject] CameraController cameraController;

    public delegate void HutStateChange(bool isOpen);
    public event HutStateChange OnHutStateChange;
    bool enteredHutTriggeredEarly;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<IPlayerController>() == null)
            return;

        //enable this section when you want the hut to stay open when the player digs into the ground
        //Debug.LogWarning(collision.transform.position + 0.5f * Vector3.up + " > " + transform.position + ((collision.transform.position.y - 0.5f) > transform.position.y).ToString());
        //if ((collision.transform.position.y+0.5f) > transform.position.y)
        Leave();
    }

    protected override void OnRealStart()
    {
        if (enteredHutTriggeredEarly)
        {
            Debug.Log("Hut transition triggered early, delayed to real start");
            Enter();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<IPlayerController>() == null)
            return;

        if (gameState.CurrentState == GameState.State.Playing)
        {
            Enter();
        }
        else
        {
            enteredHutTriggeredEarly = true;
        }
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
        OnHutStateChange?.Invoke(isOpen);

        outside_foreground.SetActive(!isOpen);
        inside_foreground.SetActive(isOpen);

        doorAudio.pitch = UnityEngine.Random.Range(0.75f, 1.5f);
        doorAudio.Play();

        if (isOpen)
            cameraController.TransitionToNewTarget(cameraTarget);
        else
            cameraController.TransitionToDefault();
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
