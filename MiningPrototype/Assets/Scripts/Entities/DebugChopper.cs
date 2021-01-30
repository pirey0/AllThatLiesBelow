using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugChopper : MonoBehaviour, IVehicle
{
    [SerializeField] new Rigidbody2D rigidbody;
    [SerializeField] new Collider2D collider;
    [SerializeField] float speed;
    [SerializeField] Transform playerPos;
    [SerializeField] SpriteRenderer spriteRenderer;

    IPlayerController player;
    private void Update()
    {
        if (player != null)
        {
            var x = player.GetHorizontalInputRaw();
            var y = player.GetVerticalInputRaw();

            transform.Translate(new Vector3(x, y) * speed * Time.deltaTime);
            if (x > 0)
                spriteRenderer.flipX = false;
            else if (x < 0)
                spriteRenderer.flipX = true;


            if (transform.position.x < 0)
            {
                transform.position = new Vector2(transform.position.x + Constants.WIDTH, transform.position.y);
            }
            else if (transform.position.x > Constants.WIDTH)
            {
                transform.position = new Vector2(transform.position.x - Constants.WIDTH, transform.position.y);
            }

            player.transform.position = playerPos.position;

            
        }   
    }

    public bool ConsumesHorizontalInput()
    {
        return true;
    }

    public bool ConsumesVerticalInput()
    {
        return true;
    }

    public void EnteredBy(IPlayerController player)
    {
        rigidbody.isKinematic = true;
        collider.isTrigger = true;
        this.player = player;
        player.Rigidbody.isKinematic = true;
    }

    public void LeftBy(IPlayerController player)
    {
        rigidbody.isKinematic = false;
        collider.isTrigger = false;
        this.player = null;
        player.Rigidbody.isKinematic = false;
    }


}
