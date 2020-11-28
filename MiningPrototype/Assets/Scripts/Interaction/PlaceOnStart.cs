using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnStart : MonoBehaviour
{
    [SerializeField] TileType type;

    private void Start()
    {
        var p = transform.position.ToGridPosition();
        RuntimeProceduralMap.Instance.SetMapAt(p.x, p.y, Tile.Make(type), TileUpdateReason.Place);
        Destroy(gameObject);
    }
}
