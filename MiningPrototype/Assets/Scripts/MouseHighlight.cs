using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHighlight : MonoBehaviour
{
    Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }
    private void FixedUpdate()
    {
        Vector3 postionRaw = camera.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
        transform.position = new Vector3((int)postionRaw.x, (int)postionRaw.y, 0) + Vector3.right * 0.5f + Vector3.up * 0.5f;
    }
}
