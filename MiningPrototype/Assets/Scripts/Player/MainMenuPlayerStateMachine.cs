using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayerStateMachine : BasePlayerStateMachine
{
    [SerializeField] float moveX;
    [SerializeField] float moveY;
    protected override float GetHorizontalInput()
    {
        return moveX;
    }

    protected override float GetVerticalInput()
    {
        return moveY;
    }
}
