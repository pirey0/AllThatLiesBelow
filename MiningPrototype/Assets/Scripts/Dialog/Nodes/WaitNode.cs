using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Nodes/Wait")]
public class WaitNode : SONode
{
    [SerializeField] float time;
    float startStamp;

    public override NodeState GetState()
    {
        if (Time.time - startStamp > time)
        {
            return NodeState.Done;
        }
        else
        {
            return NodeState.Continue;
        }
    }

    public override void Init()
    {
        startStamp = Time.time;
    }

    public override void Tick(params object[] vars)
    {
    }
}
