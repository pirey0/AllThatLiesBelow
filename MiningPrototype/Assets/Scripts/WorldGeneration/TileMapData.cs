using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "TileMapData")]
public class TileMapData : ScriptableObject
{
    [SerializeField] MapArray map;

    public Tile this[int x, int y]
    {
        get => map[x, y];
        set => map[x, y] = value;
    }

    public MapArray Map { get => map; set => map = value; }

    public void Initialize(int x, int y)
    {
        map = new MapArray(x, y);

        Util.IterateXY(x, y, (px, py) => map[px, py] = Tile.Air);
    }
}