using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MineableObject
{
    [SerializeField] float playerTouchVelocity;
    new Rigidbody2D rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.TryGetComponent(out IPlayerController psm))
        {
            Debug.Log("Hit Ball");
            rigidbody.AddForce(collision.contacts[0].normal * playerTouchVelocity, ForceMode2D.Impulse);
        }
    }

}
