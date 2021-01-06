using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : MineableObject
{
    [SerializeField] ItemDrops drops;
    protected override void Destroyed()
    {
        contains = drops.GetRandomDrop();
        base.Destroyed();
    }
}
