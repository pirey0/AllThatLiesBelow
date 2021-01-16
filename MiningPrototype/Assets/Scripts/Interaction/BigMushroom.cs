using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigMushroom : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] AudioSource audioSource;
    [SerializeField] float explosionWaitTime;

    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] PrefabFactory prefabFactory;

    Coroutine currentExplosion;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentExplosion != null)
            return;

        currentExplosion = StartCoroutine(ExplosionRoutine());
        cameraController.Shake(transform.position, CameraShakeType.raising, 3, 12, 1);
        audioSource.Play();
        animator.SetTrigger("Explode");

    }

    private IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(explosionWaitTime);
        prefabFactory.Create(explosionPrefab, transform.position + Vector3.up, Quaternion.identity);
        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
    }
}
