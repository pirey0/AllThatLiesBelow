using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarDialogRunner : StateListenerBehaviour, IInventoryOwner
{
    [SerializeField] string testDialogToRun = "Test1";

    [SerializeField] bool runOnStart;
    [SerializeField] TestDialogVisualizer dialogVisualizer;
    [SerializeField] Inventory debugInventory;

    [Zenject.Inject] ProgressionHandler progression;

    public Inventory Inventory => debugInventory;

    protected override void OnRealStart()
    {
        if (runOnStart)
            Run();
    }

    [NaughtyAttributes.Button(null, NaughtyAttributes.EButtonEnableMode.Playmode)]
    public void Run()
    {

        var collection = MiroParser.LoadTreesAsAltarTreeCollection();

        if (collection != null)
        {
            var node = collection.FindDialogWithName(testDialogToRun);
            if (node != null)
            {
                var prog = (progression == null) ? (IDialogPropertiesHandler)new TestDialogPropertiesHandler() : progression;
                INodeServiceProvider provider = new BasicDialogServiceProvider(collection, dialogVisualizer, prog, this);
                StartCoroutine(RunDialogCoroutine(provider, node));
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

    public static IEnumerator RunDialogCoroutine(INodeServiceProvider provider, AltarBaseNode node)
    {
        Debug.Log("NodeDebugRunner Start");



        NodeResult result = NodeResult.Wait;
        provider.DialogVisualizer.StartDialog();
        while (node != null && !provider.Aborted)
        {
            if (node is IConditionalNode conditionalNode)
            {
                if (!conditionalNode.ConditionsPassed(provider))
                {
                    Debug.LogError("NodeDebugRunner stopped from failed conditions " + node.ToDebugString());
                    break;
                }
            }

            if (node is IMarkIdOnRunNode)
            {
                provider.Properties.MarkRanDialog(node.ID);
            }

            if (node is IStartableNode startableNode)
            {
                result = startableNode.Start(provider);
            }

            if (result == NodeResult.Wait)
            {
                if (node is ITickingNode tickingNode)
                {
                    while (result == NodeResult.Wait && !provider.Aborted)
                    {
                        yield return null;
                        result = tickingNode.Tick(provider);
                    }
                }
            }

            if (provider.Aborted)
            {
                if (node is IEndableNode endableNode)
                    endableNode.OnEnd(provider);
                node = null;
            }
            else if (result == NodeResult.Error)
            {
                Debug.LogError("NodeDebugRunner exited with Error");
                break;
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
        provider.DialogVisualizer.EndDialog();
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
                    if (conditional.ConditionsPassed(provider))
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
}

public class BasicDialogServiceProvider : INodeServiceProvider, IDialogInventoryHandler
{
    IDialogVisualizer visualizer;
    IDialogPropertiesHandler properties;
    AltarDialogCollection treeCollection;
    IInventoryOwner inventoryOwner;

    public BasicDialogServiceProvider(AltarDialogCollection treeCollection, IDialogVisualizer vis, IDialogPropertiesHandler prop, IInventoryOwner inventoryOwner)
    {
        visualizer = vis;
        properties = prop;
        this.treeCollection = treeCollection;
        this.inventoryOwner = inventoryOwner;
    }

    public IDialogVisualizer DialogVisualizer => visualizer;
    public IDialogPropertiesHandler Properties => properties;

    public AltarDialogCollection AltarTreeCollection => treeCollection;

    public IDialogInventoryHandler DialogInventoryHandler => this;

    public bool Aborted { get; set; }

    public Inventory GetConnectedInventory()
    {
        return inventoryOwner.Inventory;
    }

    public bool InventoryConnected()
    {
        return inventoryOwner != null;
    }
}

public class TestDialogPropertiesHandler : IDialogPropertiesHandler
{
    public void FireEvent(string @event)
    {
        Debug.Log("EventFired: " + @event);
    }

    public bool GetVariable(string name)
    {
        return true;
    }

    public bool HasRunDialog(string id)
    {
        return false;
    }

    public void MarkRanDialog(string id)
    {
    }

    public void SetVariable(string variableName, bool variableState)
    {
    }
}