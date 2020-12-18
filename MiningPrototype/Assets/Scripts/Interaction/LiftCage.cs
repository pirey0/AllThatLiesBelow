using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftCage : MonoBehaviour
{
    [SerializeField] float liftSpeed = 10;
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerInteractionHandler>() != null)
            transform.Translate(GetInput() * Time.deltaTime * liftSpeed);
    }

    private Vector3 GetInput()
    {
        if (Input.GetKey(KeyCode.Q))
            return Vector3.up;
        else if (Input.GetKey(KeyCode.E))
            return Vector3.down;
        else
            return Vector3.zero;
    }
}
