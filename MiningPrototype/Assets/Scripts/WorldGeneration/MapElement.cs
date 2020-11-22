using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public interface ITileMapElement
{
    BaseMap TileMap { get; }
    void Setup(BaseMap tileMap);
}
public class MapElement : MonoBehaviour,ITileMapElement
{
    BaseMap tileMap;

    public BaseMap TileMap { get => tileMap; }

    public void Setup(BaseMap tileMap)
    {
        this.tileMap = tileMap;
    }
}
