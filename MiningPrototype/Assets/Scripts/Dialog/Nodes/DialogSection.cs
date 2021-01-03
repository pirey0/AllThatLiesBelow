using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface INode
{
    void Init();

    NodeState GetState();

    void Tick(params object[] vars);
}

public enum NodeState
{
    Continue, Error, Done
}


public class Sequence : INode
{
    List<INode> nodes = new List<INode>();
    int index;

    public NodeState GetState()
    {
        if (index >= nodes.Count)
            return NodeState.Done;
        else
            return NodeState.Continue;
    }

    public void Init()
    {
        index = 0;
        nodes[0].Init();
    }

    public void Tick(params object[] vars)
    {
        nodes[index].Tick(vars);

        if(nodes[index].GetState() == NodeState.Done)
        {
            index++;
            if(index < nodes.Count)
            {
                nodes[index].Init();
            }
        }
    }

    public void Add(INode node)
    {
        nodes.Add(node);
    }
}
