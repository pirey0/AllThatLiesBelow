using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Dynamite : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] int explostionSize = 5, destroyEntitySize;
    [SerializeField] GameObject explosionPrefab;

    [Zenject.Inject] CameraController cameraController;

    private void Start()
    {
        Invoke("Detonate", delay);
    }

    [Button]
    public void Detonate()
    {
        Vector2Int position = Util.ToGridPosition(transform.position);

        cameraController.Shake(position, shakeType: CameraShakeType.explosion, 1, explostionSize * 2 + 10);

        for (int x = -explostionSize; x <= explostionSize; x++)
        {
            for (int y = -explostionSize; y <= explostionSize; y++)
            {
                if (Vector2Int.Distance(Vector2Int.zero, new Vector2Int(x, y)) <= explostionSize)
                    RuntimeProceduralMap.Instance.DamageAt(position.x + x, position.y + y, 100, BaseMap.DamageType.Explosion);
            }
        }

        var cs = Physics2D.CircleCastAll(transform.position, destroyEntitySize, Vector2.zero);
        foreach (var collider in cs)
        {
            if (collider.transform.TryGetComponent(out TilemapCarvingEntity entity))
            {
                entity.UncarveDestroy();
            }
            else if (collider.transform.TryGetComponent(out PlayerStateMachine player))
            {
                player.TakeDamage(DamageStrength.Strong);
            }
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);//safe no injection needed
        Destroy(gameObject);
    }
}
