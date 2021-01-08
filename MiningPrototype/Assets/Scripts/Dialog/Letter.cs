using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Letter : ScriptableObject
{
    public int Id;
    public string Author;
    public int responseID;

    [TextArea(10,100)]
    public string Content;
}
