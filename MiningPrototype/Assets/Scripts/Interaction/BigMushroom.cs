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

    Coroutine currentExplosion;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentExplosion != null)
            return;

        currentExplosion = StartCoroutine(ExplosionRoutine());
        cameraController.Shake(transform.position,CameraShakeType.raising,3,12,1);
        audioSource.Play();
        animator.SetTrigger("Explode");
                
    }

    private IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(explosionWaitTime);
        Instantiate(explosionPrefab,transform.position + Vector3.up,Quaternion.identity); //Safe
        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
    }
}
