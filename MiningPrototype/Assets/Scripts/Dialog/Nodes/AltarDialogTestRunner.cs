using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarDialogTestRunner : StateListenerBehaviour
{
    [SerializeField] string dialogToRun = "Test1";

    [SerializeField] bool runOnStart;
    [SerializeField] TestDialogVisualizer dialogVisualizer;
    [SerializeField] Inventory debugInventory;

    protected override void OnRealStart()
    {
        if (runOnStart)
            Run();
    }

    [Zenject.Inject] ProgressionHandler progression;

    [NaughtyAttributes.Button(null, NaughtyAttributes.EButtonEnableMode.Playmode)]
    public void Run()
    {

        var collection = MiroParser.LoadTreesAsAltarTreeCollection();

        if (collection != null)
        {
            var node = MiroParser.FindDialogWithName(collection, dialogToRun);
            if (node != null)
            {
                StartCoroutine(RunRoutine(collection, node));
            }
            else
            {
                Debug.LogError("TestDialog not found");
            }
        }
        else
        {
            Debug.LogError("Failed to load AltarTreeCollection");
        }
    }

    public IEnumerator RunRoutine(AltarTreeCollection collection, AltarBaseNode node)
    {
        Debug.Log("NodeDebugRunner Start");
        var prog = (progression == null) ? (IDialogPropertiesHandler)new TestPropertiesHandler() : progression;
        INodeServiceProvider provider = new TestAltarDialogServiceProvider(debugInventory, dialogVisualizer, prog, collection);
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
            else
            {
                if (node is IEndableNode endableNode)
                    endableNode.OnEnd(provider);

                var newNode = SelectFirstViableChildNodeStartingAt(node, provider, (int)result);
                if (newNode == null)
                    Debug.Log("NodeDebugRunnerEnded due to Node: " + node + " " + node.ToDebugString());

                node = newNode;
                result = NodeResult.Wait;
            }
        }

        dialogVisualizer.EndDialog();
    }

    private static AltarBaseNode SelectFirstViableChildNodeStartingAt(AltarBaseNode node, INodeServiceProvider provider, int startIndex)
    {
        if (node.Children != null && node.Children.Length > 0)
        {
            //start from 0 if startIndex is negative
            int i = Mathf.Max(0, startIndex);
            while (i < node.Children.Length)
            {
                var currentChild = node.Children[i];
                if (currentChild is AltarConditionalNode conditional)
                {
                    if (conditional.ConditionPassed(provider))
                    {
                        return currentChild;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    return currentChild;
                }
            }
        }

        return null;
    }

    public class TestAltarDialogServiceProvider : INodeServiceProvider, IDialogInventoryHandler
    {
        IDialogVisualizer visualizer;
        IDialogPropertiesHandler properties;
        AltarTreeCollection treeCollection;
        Inventory inventory;

        public TestAltarDialogServiceProvider(Inventory inventory, IDialogVisualizer vis, IDialogPropertiesHandler prop, AltarTreeCollection treeCollection)
        {
            visualizer = vis;
            properties = prop;
            this.treeCollection = treeCollection;
            this.inventory = inventory;
        }

        public IDialogVisualizer DialogVisualizer => visualizer;
        public IDialogPropertiesHandler Properties => properties;

        public AltarTreeCollection AltarTreeCollection => treeCollection;

        public IDialogInventoryHandler DialogInventoryHandler => this;

        public Inventory GetConnectedInventory()
        {
            return inventory;
        }

        public bool InventoryConnected()
        {
            return inventory != null;
        }
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