using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingUIElementBase : MonoBehaviour
{
    [SerializeField] protected Vector3 targetScale;
    [SerializeField] protected float openTimeMultiplier;

    [SerializeField] protected bool updateFollow = false;
    [SerializeField] protected Transform transformToFollow;
    [SerializeField] protected Vector2 followOffset;

    private void Update()
    {
        if (updateFollow && transformToFollow != null)
        {
            UpdatePosition();
        }
    }

    protected void UpdatePosition()
    {
        transform.position = new Vector3(transformToFollow.position.x + followOffset.x, transformToFollow.position.y + followOffset.y, 0);
    }

    protected IEnumerator ScaleCoroutine(bool scaleUp = true)
    {
        while (scaleUp && transform.localScale.magnitude <= targetScale.magnitude || !scaleUp && transform.localScale.x > 0f)
        {
            transform.localScale = transform.localScale + Vector3.one * Time.deltaTime * openTimeMultiplier * (scaleUp ? 1 : -1);
            yield return null;
        }

        if (scaleUp)
            transform.localScale = targetScale;
        else
            Destroy(gameObject);
    }
}
