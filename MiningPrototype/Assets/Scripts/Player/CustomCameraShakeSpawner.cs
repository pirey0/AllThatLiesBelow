using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCameraShakeSpawner : MonoBehaviour
{
    [SerializeField] bool autoSpawn;
    [SerializeField] float spawnDelay;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float duration;
    [SerializeField] float range;
    [SerializeField] bool loop;

    [SerializeField] bool useCustomLocation;
    [SerializeField] Vector3 customLocation;

    // Start is called before the first frame update
    void Start()
    {
        if (autoSpawn)
            Spawn();
    }

    public void Spawn()
    {
        Invoke("SpawnUndelayed", spawnDelay);
    }

    public void SpawnUndelayed()
    {
        Vector3 location = useCustomLocation ? customLocation : transform.position;

        CameraController.Instance.Shake(location,curve,duration,range);

        if (loop)
            Invoke("SpawnUndelayed", duration);
    }
}
