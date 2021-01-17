using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] int explostionSize = 5, destroyEntitySize;

    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] RuntimeProceduralMap map;

    private void Start()
    {
        Vector2Int position = Util.ToGridPosition(transform.position);

        cameraController.Shake(position, shakeType: CameraShakeType.explosion, 1, explostionSize * 2 + 10);

        for (int x = -explostionSize; x <= explostionSize; x++)
        {
            for (int y = -explostionSize; y <= explostionSize; y++)
            {
                if (Vector2Int.Distance(Vector2Int.zero, new Vector2Int(x, y)) <= explostionSize)
                    map.DamageAt(position.x + x, position.y + y, 100, BaseMap.DamageType.Explosion);
            }
        }

        var cs = Physics2D.CircleCastAll(transform.position, destroyEntitySize, Vector2.zero);
        foreach (var hit in cs)
        {
            if (hit.collider.isTrigger)
                continue;

            if (hit.transform.TryGetComponent(out TilemapCarvingEntity entity))
            {
                entity.UncarveDestroy();
            }
            else if (hit.transform.TryGetComponent(out PlayerStateMachine player))
            {
                player.TakeDamage(DamageStrength.Strong);
            }
        }
    }
}
