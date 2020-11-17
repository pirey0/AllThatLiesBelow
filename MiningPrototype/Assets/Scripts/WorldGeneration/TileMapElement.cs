using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface ITileMapElement
{
    TileMap TileMap { get; }
    void Setup(TileMap tileMap);
}
public class TileMapElement : MonoBehaviour,ITileMapElement
{
    TileMap tileMap;


    public TileMap TileMap { get => tileMap; }

    public void Setup(TileMap tileMap)
    {
        this.tileMap = tileMap;
    }
}
