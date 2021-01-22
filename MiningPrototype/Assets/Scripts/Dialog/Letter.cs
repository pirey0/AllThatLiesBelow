using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Letter : ScriptableObject
{
    public int ID;
    public string Author;

    [TextArea(10, 100)]
    public string Content;

    [Header("Narrative Letter")]
    public int NextID = -1;
    public int daysToNext;
    public bool lastInSequence;
}