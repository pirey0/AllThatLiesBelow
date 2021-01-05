using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AltarPaymentNode : AltarBaseNode, IStartableNode, ITickingNode, IEndableNode
{
    public ItemAmountPair paymentRequired;

    NodeResult state = NodeResult.Wait;
    Inventory inventory;

    public NodeResult Start(INodeServiceProvider services)
    {
        state = NodeResult.Wait;
        inventory = null;

        services.DialogVisualizer.SubscribeToSelection(OnSelection);

        if (services.DialogInventoryHandler.InventoryConnected())
        {
            inventory = services.DialogInventoryHandler.GetConnectedInventory();
            inventory.InventoryChanged += OnInventoryChanged;
            services.DialogVisualizer.DisplayOptions(new string[] { "another time..." });

            //Call manually to trigger payment if player placed premptively, without needing an InventoryChanged event
            OnInventoryChanged(true, ItemAmountPair.Nothing, false);
            return NodeResult.Wait;
        }
        else
        {
            Debug.LogError("No connected inventory for AltarPayment Node");
            return NodeResult.Error;
        }
    }

    private void OnInventoryChanged(bool add, ItemAmountPair element, bool playsound)
    {
        if (state == NodeResult.Wait && inventory.Contains(paymentRequired))
        {
            //First == Payed succesfully
            state = NodeResult.First;
        }
    }

    private void OnSelection(int obj)
    {
        //Second == Cancel
        state = NodeResult.Second;
    }

    public NodeResult Tick(INodeServiceProvider services)
    {
        if (state == NodeResult.First)
        {
            Debug.Log("Payment Node paying: " + paymentRequired.ToString());
            bool success = inventory.TryRemove(paymentRequired);
            if (!success)
                Debug.LogError("Payment Failed");
        }

        return state;
    }

    public void OnEnd(INodeServiceProvider services)
    {
        inventory.InventoryChanged -= OnInventoryChanged;
        services.DialogVisualizer.UnsubscribeFromSelection(OnSelection);
        services.DialogVisualizer.Clear();
    }
}

public class AltarSelectionChoiceNode : AltarChoiceNode, IStartableNode, ITickingNode, IEndableNode
{
    public override NodeResult Start(INodeServiceProvider services)
    {
        List<AltarBaseNode> choiceNodes = new List<AltarBaseNode>();

        foreach (var item in services.AltarTreeCollection.Roots)
        {
            if (item is AltarOptionNode)
            {
                choiceNodes.Add(item);
            }
        }

        Children = choiceNodes.ToArray();
        return base.Start(services);
    }
}

public class AltarChoiceNode : AltarBaseNode, IStartableNode, ITickingNode, IEndableNode
{
    int selected = -1;
    private List<int> mapOptionsIndexToChildrenId;


    private void OnSelect(int obj)
    {
        selected = obj;
    }

    public virtual NodeResult Start(INodeServiceProvider services)
    {
        selected = -1;
        services.DialogVisualizer.SubscribeToSelection(OnSelect);

        var options = CalculateOptions(services);

        if (options.Length == 0)
        {
            Debug.LogError("No Options available to ChoiceNode");
            return NodeResult.Error;
        }

        services.DialogVisualizer.DisplayOptions(options);

        return NodeResult.Wait;
    }

    private string[] CalculateOptions(INodeServiceProvider services)
    {
        mapOptionsIndexToChildrenId = new List<int>();
        List<string> options = new List<string>();

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i] is AltarOptionNode optionNode)
            {
                if (optionNode.ConditionsPassed(services))
                {
                    options.Add(optionNode.OptionText);
                    mapOptionsIndexToChildrenId.Add(i);
                }
            }
        }

        return options.ToArray();
    }



    public NodeResult Tick(INodeServiceProvider services)
    {
        if (selected == -1)
            return NodeResult.Wait;

        if (selected >= mapOptionsIndexToChildrenId.Count)
        {
            Debug.LogError("Option selected is out of bounds");
            return NodeResult.Error;
        }
        else
        {
            return (NodeResult)mapOptionsIndexToChildrenId[selected];
        }
    }

    public void OnEnd(INodeServiceProvider services)
    {
        services.DialogVisualizer.UnsubscribeFromSelection(OnSelect);
    }
}

public class AltarOptionNode : AltarConditionalNode
{
    public string OptionText;

    public override string ToDebugString()
    {
        return "Option: " + OptionText + base.ToDebugString();
    }
}


[System.Serializable]
public class AltarDialogRootNode : AltarConditionalNode
{
    public string Name;
    public bool IsEncounter;

    public override string ToDebugString()
    {
        return "Root: " + Name + base.ToDebugString();
    }
}

public class AltarConditionalNode : AltarBaseNode, IConditionalNode, IMarkIdOnRunNode
{
    public List<INodeRequirement> Requirements = new List<INodeRequirement>();

    public virtual bool ConditionsPassed(INodeServiceProvider services)
    {
        foreach (var r in Requirements)
        {
            if (!r.RequirementPassed(services))
                return false;
        }

        return true;
    }

    public override string ToDebugString()
    {
        string s = "";
        foreach (var r in Requirements)
        {
            s += r.ToDebugString() + " ";
        }

        s += base.ToDebugString();
        return s;
    }
}

public class AltarVariableRequirement : INodeRequirement
{
    string name;
    bool state;

    public AltarVariableRequirement(string name, bool state)
    {
        this.name = name;
        this.state = state;
    }

    public bool RequirementPassed(INodeServiceProvider services)
    {
        return services.Properties.GetVariable(name) == state;
    }

    public string ToDebugString()
    {
        return "Req: " + name + " " + state;
    }
}

public class AltarRunOnceOnlyRequirement : INodeRequirement
{
    public string sourceId;

    public AltarRunOnceOnlyRequirement(string sourceID)
    {
        sourceId = sourceID;
    }

    public bool RequirementPassed(INodeServiceProvider services)
    {
        return !services.Properties.HasRunDialog(sourceId);
    }

    public string ToDebugString()
    {
        return "RunOnlyOnce";
    }
}


[System.Serializable]
public class AltarTextNode : AltarBaseNode, IStartableNode, ITickingNode, IEndableNode
{
    public string Text;
    bool finished = false;

    public IDialogVisualizer DialogVisualizer { get; set; }

    public void OnEnd(INodeServiceProvider services)
    {
        services.DialogVisualizer.UnsubscribeFromSelection(OnFinished);
    }

    public NodeResult Start(INodeServiceProvider services)
    {
        finished = false;
        services.DialogVisualizer.SubscribeToSelection(OnFinished);
        services.DialogVisualizer.DisplaySentence(Text);
        return NodeResult.Wait;
    }

    private void OnFinished(int obj)
    {
        finished = true;
    }

    public NodeResult Tick(INodeServiceProvider services)
    {
        return finished ? NodeResult.First : NodeResult.Wait;
    }

    public override string ToDebugString()
    {
        return "\"" + Text + "\"" + base.ToDebugString();
    }
}

[System.Serializable]
public class AltarSetVariableNode : AltarBaseNode, IStartableNode
{
    public string VariableName;
    public bool VariableState;

    public NodeResult Start(INodeServiceProvider services)
    {
        services.Properties.SetVariable(VariableName, VariableState);
        return NodeResult.First;
    }

    public override string ToDebugString()
    {
        return "Set: " + VariableName + ": " + VariableState + base.ToDebugString();
    }
}

[System.Serializable]
public class AltarEventTriggerNode : AltarBaseNode, IStartableNode
{
    public string Event;

    public NodeResult Start(INodeServiceProvider services)
    {
        services.Properties.FireEvent(Event);
        return NodeResult.First;
    }

    public override string ToDebugString()
    {
        return "Event: " + Event + base.ToDebugString();
    }
}

