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

    //Up Right Down Left
    public int[] Stabilities;

    public int Stability { get => Stabilities.Sum(); }

    public static Tile Air
    {
        get
        {
            var t = new Tile();
            t.Type = TileType.Air;
            t.NeighbourBitmask = 0;
            t.Damage = 0;
            t.Visibility = 0;
            t.Stabilities = new int[4];
            return t;
        }
    }

    public static Tile Make(TileType t)
    {
        Tile tile = Air;
        tile.Type = t;
        return tile;
    }

    public void SetStability(Direction dir, int value)
    {
        Stabilities[(int)dir] = value;
    }

    public void ReduceStabilityBy(int i)
    {
        Stabilities[(int)Direction.Down] -= i;
    }

    public void ResetStability()
    {
        Stabilities = new int[4];
    }

    public bool StableWithout(Direction dir)
    {
        return Type == TileType.Air || (Stability-Stabilities[(int)dir] >= 100 );
    }

    public void TakeDamage(float amount)
    {
        Damage += amount;
    }

    public override string ToString()
    {
        return Type.ToString() + " [N: " + NeighbourBitmask + " D:" + Damage.ToString("n1") + " S: " +Stability + " (" +  string.Join(", " , Stabilities) + ")]"; 
    }
}

public enum Direction
{
    Up, Right, Down, Left
}


public interface ITileUpdateReceiver
{
    void OnTileCrumbleNotified(int x, int y);
    void OnTileChanged(int x, int y, TileUpdateReason reason);
    void OnTileUpdated(int x, int y, TileUpdateReason reason);
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
