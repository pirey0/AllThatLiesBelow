using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody;

    [SerializeField] float groundedAngle;
    [SerializeField] float jumpVelocity;
    [SerializeField] float moveSpeed;

    [SerializeField] float jumpCooldown = 0.1f;
    [SerializeField] float timeAfterGroundedToJump = 0.1f;

    [SerializeField] Transform feet;
    [SerializeField] float feetRadius;

    [SerializeField] TestGeneration generation;

    float lastGroundedTimeStamp;
    float lastJumpTimeStamp;

    private bool isGrounded;
    Vector2 rightWalkVector = Vector3.right;
    Camera camera;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        camera = Camera.main;
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private bool CanJump()
    {
        return Time.time - lastGroundedTimeStamp < timeAfterGroundedToJump && Time.time - lastJumpTimeStamp > jumpCooldown;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 clickPos = camera.ScreenToWorldPoint(Input.mousePosition);
            generation.TryCarve((int)clickPos.x, (int)clickPos.y);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 clickPos = camera.ScreenToWorldPoint(Input.mousePosition);
            generation.TryPlace((int)clickPos.x, (int)clickPos.y);
        }
    }

    private void FixedUpdate()
    {
        UpdateWalk();
        UpdateJump();
    }

    private void UpdateWalk()
    {
        var horizontal = Input.GetAxis("Horizontal");

        rigidbody.position += horizontal * rightWalkVector * moveSpeed * Time.fixedDeltaTime;
        rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);

        if(Mathf.Abs(horizontal) > 0.2f)
            spriteRenderer.flipX = horizontal < 0;
    }

    private void UpdateJump()
    {
        var vertical = Input.GetAxis("Vertical");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(feet.position, feetRadius);
        isGrounded = colliders != null && colliders.Length > 1;

        if (isGrounded)
        {
            lastGroundedTimeStamp = Time.time;
        }

        if (CanJump() && vertical > 0)
        {
            Jump();
        }
    }

    private void Jump()
    {
        Debug.Log("Jump");
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpVelocity);
        lastJumpTimeStamp = Time.time;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateWalkVector(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateWalkVector(collision);
    }

    private void UpdateWalkVector(Collision2D collision)
    {
        var contact = collision.contacts[0];
        float angle = Mathf.Acos(Vector3.Dot(contact.normal, Vector3.up)) * Mathf.Rad2Deg;

        Debug.DrawLine(transform.position, transform.position + (Vector3)contact.normal);

        if (angle < groundedAngle)
        {
            rightWalkVector = Vector3.Cross(contact.normal, Vector3.forward).normalized;
        }
        else
        {
            rightWalkVector = Vector3.right;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (feet != null)
            Gizmos.DrawWireSphere(feet.position, feetRadius);

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightWalkVector);
    }
}
