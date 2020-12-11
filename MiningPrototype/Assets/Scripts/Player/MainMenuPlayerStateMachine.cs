using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayerStateMachine : BasePlayerStateMachine
{
    [Header("MainMenu Player")]
    [SerializeField] float moveX;
    [SerializeField] float moveY;

    [SerializeField] float mapWidth;

    protected override float GetHorizontalInput()
    {
        return moveX;
    }

    protected override float GetVerticalInput()
    {
        return moveY;
    }

    protected override void BaseMoveUpdate(float horizontal, Vector2 movement)
    {
        base.BaseMoveUpdate(horizontal, movement);

        if (rigidbody.position.x < 0)
        {
            rigidbody.position = new Vector2(rigidbody.position.x + mapWidth, rigidbody.position.y);
        }
        else if (rigidbody.position.x > mapWidth)
        {
            rigidbody.position = new Vector2(rigidbody.position.x - mapWidth, rigidbody.position.y);
        }
    }
}
