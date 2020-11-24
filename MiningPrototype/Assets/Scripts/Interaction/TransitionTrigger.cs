using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [SerializeField] Transform target;

    [Zenject.Inject] CameraController cameraController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent(out PlayerInteractionHandler pc))
        {
            if(target != null)
            {
                cameraController.TransitionToNewTarget(target);
            }
        }
    }
}
