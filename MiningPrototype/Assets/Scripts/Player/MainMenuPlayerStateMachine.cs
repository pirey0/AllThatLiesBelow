using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayerStateMachine : BasePlayerStateMachine
{
    [SerializeField] float moveX;
    [SerializeField] float moveY;
    [SerializeField] bool inPlace;
    Vector3 position;
    protected override float GetHorizontalInput()
    {
        return moveX;
    }

    protected override float GetVerticalInput()
    {
        return moveY;
    }

    //hold the player in place to prevent him from falling down
    private void Update()
    {
        if (inPlace)
        {
            if (position != Vector3.zero)
                transform.position = position;

            position = transform.position;
        }
    }
}
