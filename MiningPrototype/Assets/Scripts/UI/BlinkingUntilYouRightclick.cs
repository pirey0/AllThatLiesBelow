using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingUntilYouRightclick : BlinkingForSpriteRenderer
{
    [Zenject.Inject] CameraController cameraController;
    protected override void Start()
    {
        base.Start();
        StartBlinking();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Vector3.Distance(Util.MouseToWorld(cameraController.Camera), transform.position) < 1)
        {
            StopBlinking();
            Destroy(this);
        }
    }
}
