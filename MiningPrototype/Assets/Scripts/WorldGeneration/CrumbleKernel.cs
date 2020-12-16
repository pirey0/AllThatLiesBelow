using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum CrumbleType
{
    Null = 0,
    Crumble = 1,
    Air = 2,
    Stone = 4,
    Ignore = 8,

}


[System.Serializable]
public class Kernel
{
    CrumbleType[,] map;
    int width, height;

    public int Width { get => width; }
    public int Height { get => height; }
    public CrumbleType this[int x, int y] { get => map[x, y]; set => TrySet(x, y, value); }

    public void TrySet(int x, int y, CrumbleType type)
    {
        if(x<0 || y>= width ||y <0 || y >= height)
        {
            return;
        }

        map[x, y] = type;
    }

    public Kernel(int width, int height)
    {
        this.width = width;
        this.height = height;
        map = new CrumbleType[width, height];
    }

    //Constructor from string Array meant for parsing text file with kernels
    public static Kernel FromStrings(string[] str)
    {
        if (str.Length == 0)
            return null;

        Kernel k = new Kernel(str[0].Length, str.Length);


        for (int y = 0; y < k.Height; y++)
        {
            for (int x = 0; x < k.Width; x++)
            {
                k[x, y] = CharToCrumbleType(str[y][x]);
            }
        }

        return k;
    }


    public static CrumbleType CharToCrumbleType(char c)
    {
        
        switch (c)
        {
            case '0':
                return CrumbleType.Air;
            case '1':
                return CrumbleType.Stone;
            case 'I':
            case 'i':
                return CrumbleType.Ignore;
            case 'c':
            case 'C':
                return CrumbleType.Crumble;

            default:
                Debug.LogError("Found unknown character: " + c + " " + (int)c);
                return CrumbleType.Null;
        }
    }

}
