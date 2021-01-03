using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionNode : SONode
{

    [SerializeField] string[] options;

    [Zenject.Inject] IDialogVisualizer visualizer;

    bool done = false;

    public override NodeState GetState()
    {
        if (done)
            return NodeState.Done;
        else
            return NodeState.Continue;
    }

    public override void Init()
    {
        done = false;
    }

    public override void Tick(params object[] vars)
    {
    }
}

