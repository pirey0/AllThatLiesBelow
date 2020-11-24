using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblingParticle : MonoBehaviour
{
    [SerializeField] CameraShakeType type;
    [SerializeField] float range;
    [SerializeField] AudioSource crumblingSound;

    [Zenject.Inject] CameraController cameraController;

    CameraShake shake;

    float duration;

    private void OnDestroy()
    {
        cameraController.StopShake(shake);
    }

    public void SetDuration(float f)
    {
        duration = f;

        if (duration <= 0)
            return;

        if (duration > 10)
            Debug.LogError("Duration greater than 10: " + f);

        crumblingSound.time = Mathf.Max(0f, 10f - duration);
        shake = cameraController.Shake(transform.position, type, duration, range);
    }
}
