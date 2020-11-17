using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeSpawner : MonoBehaviour
{
    [SerializeField] CameraShakeType type;
    [SerializeField] float length;
    [SerializeField] float range;
    CameraShake shake;

    // Start is called before the first frame update
    void Start()
    {
        shake = CameraController.Instance.Shake(transform.position,type,length, range);
    }

    private void OnDestroy()
    {
        CameraController.Instance.StopShake(shake);
    }
}
