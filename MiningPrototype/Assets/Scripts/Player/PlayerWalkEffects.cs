using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkEffects : MonoBehaviour
{
    [SerializeField] float amountOnWalk;
    [SerializeField] new ParticleSystem particleSystem;
    [SerializeField] ParticleSystem.MinMaxGradient snow, stone, grayStone, grass;
    [Zenject.Inject] RuntimeProceduralMap map;
    
    public void SetEffects(bool showEffects)
    {
        var emit = particleSystem.emission;
        var main = particleSystem.main;

        float amount = 0;

        if (showEffects)
        {
            var pos = transform.position.ToGridPosition() + new Vector2Int(0, 0);
            TileType type = map.GetTileAt(pos.x, pos.y).Type;

            if (type != TileType.Air)
            {
                amount = amountOnWalk;
                main.startColor = GetColorForType(type);
            }
        }

        emit.rateOverTime = amount;
    }

    private ParticleSystem.MinMaxGradient GetColorForType(TileType type)
    {
        switch (type)
        {
            case TileType.Snow:
                return snow;

            case TileType.Grass:
                return grass;

            case TileType.HardStone:
            case TileType.Rock:
            case TileType.FillingStone:
                return grayStone;

            default:
                return stone;
        }
    }
}
