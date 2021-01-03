using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarDialogTestRunner : StateListenerBehaviour
{
    [SerializeField] string dialogToRun = "Test1";

    [SerializeField] bool runOnStart;
    [SerializeField] TestDialogVisualizer dialogVisualizer;

    protected override void OnRealStart()
    {
        if (runOnStart)
            Run();
    }

    [Zenject.Inject] ProgressionHandler progression;

    [NaughtyAttributes.Button(null, NaughtyAttributes.EButtonEnableMode.Playmode)]
    public void Run()
    {

        var node = MiroParser.GetTestAltarDialogWithName(dialogToRun);

        if (node != null)
        {
            StartCoroutine(RunRoutine(node));
        }
        else
        {
            Debug.LogError("TestDialog was Null");
        }
    }

    public IEnumerator RunRoutine(AltarBaseNode node)
    {
        Debug.Log("NodeDebugRunner Start");
        INodeServiceProvider provider = new TestAltarDialogServiceProvider(dialogVisualizer, (progression == null) ? (IDialogPropertiesHandler)new TestPropertiesHandler() : progression); ;
        NodeResult result = NodeResult.Wait;
        dialogVisualizer.StartDialog();
        while (node != null)
        {
            if (node is IConditionalNode conditionalNode)
            {
                if (!conditionalNode.ConditionPassed(provider))
                {
                    Debug.LogError("NodeDebugRunner stopped from failed conditions " + node.ToDebugString());
                    yield break;
                }
            }

            if (node is IStartableNode startableNode)
            {
                result = startableNode.Start(provider);
            }

            if (result == NodeResult.Wait)
            {
                if (node is ITickingNode tickingNode)
                {
                    while (result == NodeResult.Wait)
                    {
                        yield return null;
                        result = tickingNode.Tick(provider);
                    }
                }
            }

            if (result == NodeResult.Error)
            {
                Debug.LogError("NodeDebugRunner exited with Error");
                yield break;
            }
            else if ((int)result < node.Children.Length && (int)result >= 0)
            {
                if (node is IEndableNode endableNode)
                {
                    endableNode.OnEnd(provider);
                }

                node = node.Children[(int)result];
                result = NodeResult.Wait;
            }
            else
            {
                if (node is IEndableNode endableNode)
                    endableNode.OnEnd(provider);

                if (node.Children != null && node.Children.Length > 0)
                {
                    node = node.Children[0];
                }
                else
                {
                    Debug.Log("Result: " + result + ", Node: " + node + " " + node.ToDebugString());
                    node = null;
                }
                result = NodeResult.Wait;
            }
        }

        dialogVisualizer.EndDialog();
        Debug.Log("NodeDebugRunner Finish");
    }


    public class TestAltarDialogServiceProvider : INodeServiceProvider
    {
        IDialogVisualizer visualizer;
        IDialogPropertiesHandler properties;
        public TestAltarDialogServiceProvider(IDialogVisualizer vis, IDialogPropertiesHandler prop)
        {
            visualizer = vis;
            properties = prop;
        }

        public IDialogVisualizer DialogVisualizer => visualizer;
        public IDialogPropertiesHandler Properties => properties;
    }

    public class TestPropertiesHandler : IDialogPropertiesHandler
    {
        public void FireEvent(string @event)
        {
            Debug.Log("EventFired: " + @event);
        }

        public bool GetVariable(string name)
        {
            return true;
        }

        public void SetVariable(string variableName, bool variableState)
        {
        }
    }
}