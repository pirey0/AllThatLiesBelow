using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MineableObject
{
    [SerializeField] EdgeCollider2D edge;

    public void NotifyGoingDown()
    {
        edge.enabled = false;
    }

    public void NotifyGoingUp()
    {
        edge.enabled = true;
    }

}
