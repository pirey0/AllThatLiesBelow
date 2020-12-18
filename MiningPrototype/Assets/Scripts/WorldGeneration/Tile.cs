using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum TileType
{
    Air = 0,
    Stone = 1,
    Copper = 2,
    Gold = 3,
    Snow = 4,
    Diamond = 5,
    HardStone = 6,
    BedStone = 7,
    Rock,
    CollapsableEntity,
    FloatingEntity,
    CollapsableEntityNotNeighbour,
    FloatingEntityNotNeighbour,
    Ignore,
    Grass,
    SolidVoid,
    Platform,
    FillingStone
}

[System.Serializable]
public struct Tile
{
    public TileType Type;
    public byte NeighbourBitmask;
    public float Damage;

    public int Visibility;
    public bool Discovered;
    public bool Unstable;

    public static Tile Air
    {
        get
        {
            var t = new Tile();
            t.Type = TileType.Air;
            t.NeighbourBitmask = 0;
            t.Damage = 0;
            t.Visibility = 0;
            t.Discovered = false;
            t.Unstable = false;
            return t;
        }
    }

    public static Tile Make(TileType t)
    {
        Tile tile = Air;
        tile.Type = t;
        return tile;
    }

    public void TakeDamage(float amount)
    {
        Damage += amount;
    }

    public override string ToString()
    {
        return Type.ToString() + "[S: "+ !Unstable + " N: " + NeighbourBitmask + " Dmg:" + Damage.ToString("n1") + " Dis: " + Discovered + "]" ; 
    }
}

public enum Direction
{
    Up, Right, Down, Left, None = -1
}


public interface ITileUpdateReceiver
{
    void OnTileCrumbleNotified(int x, int y);
    void OnTileChanged(int x, int y, TileUpdateReason reason);
    void OnTileUpdated(int x, int y);
    GameObject gameObject { get; }
}

[Flags]
public enum TileUpdateReason
{
    None =0,
    VisualUpdate=1,
    Destroy=2,
    Collapse=4,
    Uncarve=8,
    Carve=16,
    Place=32,
    Generation=64,
    MapLoad= 128,
    DoNotUpdateReceivers = Generation | Carve
}
