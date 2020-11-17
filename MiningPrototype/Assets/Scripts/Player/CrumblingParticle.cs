using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblingParticle : MonoBehaviour
{
    [SerializeField] CameraShakeType type;
    [SerializeField] float range;
    CameraShake shake;

    float duration;

    // Start is called before the first frame update
    void Start()
    {
        shake = CameraController.Instance.Shake(transform.position,type, duration, range);
    }

    private void OnDestroy()
    {
        CameraController.Instance.StopShake(shake);
    }

    public void SetDuration(float f)
    {
        duration = f;
    }
}
