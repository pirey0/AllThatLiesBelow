using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeDebugRunner : MonoBehaviour
{
    [SerializeField] SONode[] nodes;

    INode node;

    private void Start()
    {
        Sequence sequence = new Sequence();

        foreach (var n in nodes)
        {
            sequence.Add(n);
        }
        node = sequence;
    }

    [NaughtyAttributes.Button]
    public void Run()
    {
        StartCoroutine(RunRoutine());
    }

    public IEnumerator RunRoutine()
    {
        Debug.Log("NodeDebugRunner Start");
        node.Init();
        bool done = false;
        while (!done)
        {
            node.Tick();
            if (node.GetState() == NodeState.Done || node.GetState() == NodeState.Error)
            {
                done = true;
            }
            yield return null;
        }
        Debug.Log("NodeDebugRunner Finish");
    }
}