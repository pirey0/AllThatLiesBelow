using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Nodes/Log")]
public class LogNode : SONode
{
    [SerializeField] string msg;

    public override NodeState GetState()
    {
        return NodeState.Done;
    }

    public override void Init()
    {
        Debug.Log(msg);
    }

    public override void Tick(params object[] vars)
    {
    }
}


public abstract class SONode : ScriptableObject, INode
{
    public abstract NodeState GetState();

    public abstract void Init();

    public abstract void Tick(params object[] vars);
}