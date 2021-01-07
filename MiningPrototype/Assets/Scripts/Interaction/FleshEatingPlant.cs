using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshEatingPlant : BasicNonPersistantSavable
{
    [SerializeField] SpriteAnimator spriteAnimator;
    [SerializeField] SpriteAnimation walkOver, snap;

    [SerializeField] float snapDelay, snapAnimationTilSnap, snapAnimationRestDuration;

    [Zenject.Inject] PlayerStateMachine playerStateMachine;
    [Zenject.Inject] CameraController cameraController;

    bool activated = false;
    bool inSnapping = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!inSnapping)
        {
            if (spriteAnimator.IsDone())
                spriteAnimator.Play(walkOver);

            if (!activated)
            {
                activated = true;
                StartCoroutine(SnappingRoutine());
            }
        }
    }

    IEnumerator SnappingRoutine()
    {
        cameraController.Shake(transform.position,shakeType:CameraShakeType.raising,snapDelay,10,1);
        yield return new WaitForSeconds(snapDelay - snapAnimationTilSnap);
        spriteAnimator.Play(snap);
        yield return new WaitForSeconds(snapAnimationTilSnap);
        inSnapping = true;
        yield return new WaitForSeconds(snapAnimationRestDuration);
        inSnapping = false;
        activated = false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (inSnapping)
        {
            if (collision.gameObject.GetComponent<PlayerInteractionHandler>() != null)
                playerStateMachine.TakeDamage(DamageStrength.Strong);
        }
    }
}
