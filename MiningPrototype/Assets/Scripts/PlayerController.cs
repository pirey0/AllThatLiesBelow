using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody;

    [SerializeField] float groundedAngle;
    [SerializeField] float jumpVelocity;
    [SerializeField] float moveSpeed;

    [SerializeField] Transform feet;
    [SerializeField] float feetRadius;

    float lastGroundedTimeStamp;

    private bool isGrounded;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        Collider2D[] colliders = Physics2D.OverlapCircleAll(feet.position, feetRadius);
        isGrounded = colliders != null && colliders.Length > 1;

        if (isGrounded)
        {
            lastGroundedTimeStamp = Time.time;
        }

        if(isGrounded && vertical > 0)
        {
            rigidbody.AddForce(Vector2.up * jumpVelocity, ForceMode2D.Impulse);
        }

        rigidbody.position += horizontal * Vector2.right * moveSpeed * Time.fixedDeltaTime;
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {

        foreach (var contact in collision.contacts)
        {
            float angle = Mathf.Acos(Vector3.Dot(contact.normal, Vector3.up)) * Mathf.Rad2Deg;

            Debug.DrawLine(transform.position, transform.position + (Vector3)contact.normal);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(feet != null)
        Gizmos.DrawWireSphere(feet.position, feetRadius);
    }
}
