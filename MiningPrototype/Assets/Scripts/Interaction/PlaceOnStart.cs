using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnStart : MonoBehaviour
{
    [SerializeField] TileType type;

    [Zenject.Inject] RuntimeProceduralMap map;
    private void Start()
    {
        var p = transform.position.ToGridPosition();
        map.SetMapAt(p.x, p.y, Tile.Make(type), TileUpdateReason.Place);
        Destroy(gameObject);
    }
}
