using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarDialogRunner : StateListenerBehaviour
{
    [SerializeField] string testDialogToRun = "Selection";

    [SerializeField] bool runOnStart;
    [SerializeField] TestDialogVisualizer dialogVisualizer;
    [SerializeField] GameObject debugInventory;

    [Zenject.Inject] ProgressionHandler progression;


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
                BasicDialogServiceProvider provider = new BasicDialogServiceProvider(progression);
                provider.SetVisualizer(dialogVisualizer);
                provider.SetInventory(debugInventory);
                StartCoroutine(DialogCoroutine(provider, node));
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

    public static IEnumerator DialogCoroutine(INodeServiceProvider provider, AltarBaseNode node, System.Action finishedCallback = null)
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
        finishedCallback?.Invoke();
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

public class BasicDialogServiceProvider : INodeServiceProvider
{
    IDialogVisualizer visualizer;
    IDialogPropertiesHandler properties;
    GameObject inventoryGO;
    Inventory inventory;

    public BasicDialogServiceProvider(IDialogPropertiesHandler prop)
    {
        properties = prop;
    }

    public void SetVisualizer(IDialogVisualizer vis)
    {
        visualizer = vis;
    }

    public void SetInventory(GameObject inventoryGO)
    {
        this.inventoryGO = inventoryGO;
        if(inventoryGO.TryGetComponent(out IInventoryOwner owner))
        {
            inventory = owner.Inventory;
        }
    }

    public Inventory SpawnInventory()
    {
        inventoryGO.SetActive(true);
        return inventory;
    }

    public void DestroyInventory()
    {
        inventoryGO.SetActive(false);
    }

    public IDialogVisualizer DialogVisualizer => visualizer;
    public IDialogPropertiesHandler Properties => properties;

    public bool Aborted { get; set; }

}

