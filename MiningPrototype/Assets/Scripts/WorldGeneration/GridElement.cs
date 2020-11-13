using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    TileMap tileMap;


    public TileMap TileMap { get => tileMap; }

    public void Setup(TileMap tileMap)
    {
        this.tileMap = tileMap;
    }
}
