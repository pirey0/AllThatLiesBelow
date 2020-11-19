using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesData
{
    static Dictionary<TileType, TileInfo> tileInfos;

    public static Dictionary<TileType, TileInfo> TileInfos { get => tileInfos; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void FindTileInfos()
    {
        tileInfos = new Dictionary<TileType, TileInfo>();

        TileInfo[] tiles = Resources.LoadAll<TileInfo>("Tiles");

        Debug.Log("Loaded " + tiles.Length + " tileInfos");

        foreach (var item in tiles)
        {
            if (tileInfos.ContainsKey(item.Type))
            {
                Debug.LogError("Duplicate declaration for " + item.Type);
            }
            else
            {
                tileInfos.Add(item.Type, item);
            }
        }
    }

    public static TileInfo GetTileInfo(TileType itemType)
    {
        if (tileInfos == null)
            FindTileInfos();

        if (tileInfos.ContainsKey(itemType))
        {
            return tileInfos[itemType];
        }
        return null;
    }
}
