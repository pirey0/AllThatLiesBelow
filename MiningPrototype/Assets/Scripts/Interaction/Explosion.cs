using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Explosion : MonoBehaviour
{
    [SerializeField] int explostionSize = 5, destroyEntitySize;
    [SerializeField] Light2D light2D;
    [SerializeField] AnimationCurve light2DIntensityOverTime;

    [Zenject.Inject] CameraController cameraController;
    [Zenject.Inject] RuntimeProceduralMap map;

    float timeOnStart = 0;

    private void Start()
    {
        timeOnStart = Time.time;

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

    private void Update()
    {
        light2D.intensity = light2DIntensityOverTime.Evaluate(Time.time - timeOnStart);
    }
}
