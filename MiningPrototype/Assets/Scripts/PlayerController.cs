using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody;

    [SerializeField] float groundedAngle;
    [SerializeField] float jumpVelocity;
    [SerializeField] float moveSpeed;

    float groundedTimeStamp;

    private bool isGrounded { get => Time.time - groundedTimeStamp < 0.1f; }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");


        if(isGrounded && vertical > 0)
        {
            rigidbody.AddForce(Vector2.up * jumpVelocity, ForceMode2D.Impulse);
        }

        rigidbody.position += horizontal * Vector2.right * moveSpeed * Time.fixedDeltaTime;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var p = collision.contacts[0];

        float angle = Mathf.Acos(Vector3.Dot(((Vector3)p.point - transform.position).normalized, Vector3.down)) * Mathf.Rad2Deg;

        if(angle < groundedAngle)
        {
            groundedTimeStamp = Time.time;
        }

    }

}
