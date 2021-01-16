using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Dynamite : MirrorWorldFollower
{
    [SerializeField] float delay;
    [SerializeField] GameObject explosionPrefab;

    [Zenject.Inject] PrefabFactory prefabFactory;

    private void Start()
    {
        Invoke("Detonate", delay);
    }

    [Button]
    public void Detonate()
    {
        prefabFactory.Create(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
