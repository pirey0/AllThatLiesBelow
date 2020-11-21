using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocationIndicator : MonoBehaviour
{
    public IndicatorType Type;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Util.GizmosDrawTile(transform.position.ToGridPosition());
    }

    public static LocationIndicator Find(IndicatorType type)
    {
        return GameObject.FindObjectsOfType<LocationIndicator>().First((x) => x.Type == type);
    }
    
}
public enum IndicatorType
{
    PlayerStart,
    OrderSpawn
}
