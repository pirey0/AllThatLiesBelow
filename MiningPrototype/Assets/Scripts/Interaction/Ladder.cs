using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
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
