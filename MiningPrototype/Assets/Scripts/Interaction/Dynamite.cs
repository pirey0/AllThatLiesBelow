using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Dynamite : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] int explostionSize = 5;
    [SerializeField] GameObject explosionPrefab;

    [Zenject.Inject] CameraController cameraController;

    private void Start ()
    {
        Invoke("Detonate",delay);
    }

    [Button]
    public void Detonate()
    {
        Vector2Int position = Util.ToGridPosition(transform.position);

        cameraController.Shake(position, shakeType: CameraShakeType.explosion, 1, explostionSize * 2 + 10);

        for (int x = - explostionSize; x <= explostionSize; x++)
        {
            for (int y = - explostionSize; y <= explostionSize; y++)
            {
                if (Vector2Int.Distance(Vector2Int.zero,new Vector2Int(x,y)) <= explostionSize)
                RuntimeProceduralMap.Instance.DamageAt(position.x + x, position.y + y, 100, playerCaused: true);
            }
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
