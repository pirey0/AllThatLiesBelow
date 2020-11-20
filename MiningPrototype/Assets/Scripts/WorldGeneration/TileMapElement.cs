using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public interface ITileMapElement
{
    Map TileMap { get; }
    void Setup(Map tileMap);
}
public class TileMapElement : MonoBehaviour,ITileMapElement
{
    Map tileMap;


    public Map TileMap { get => tileMap; }

    public void Setup(Map tileMap)
    {
        this.tileMap = tileMap;
    }
}
