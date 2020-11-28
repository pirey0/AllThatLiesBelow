using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MineableObject
{    
    private void Start ()
    {
        //torches are in the background (0.9f)
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.9f);
    }
}
