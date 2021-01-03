using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AltarChoiceNode : AltarBaseNode, IStartableNode, ITickingNode, IEndableNode
{
    int selected = -1;
    private List<int> mapOptionsIndexToChildrenId;


    private void OnSelect(int obj)
    {
        selected = obj;
    }

    public NodeResult Start(INodeServiceProvider services)
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
                options.Add(optionNode.OptionText);
                mapOptionsIndexToChildrenId.Add(i);
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

    public override string ToDebugString()
    {
        return "Root: " + Name + base.ToDebugString();
    }
}

public class AltarConditionalNode : AltarBaseNode, IConditionalNode
{
    public List<(string, bool)> Requirements = new List<(string, bool)>();

    public virtual bool ConditionPassed(INodeServiceProvider services)
    {
        foreach (var r in Requirements)
        {
            if (services.Properties.GetVariable(r.Item1) != r.Item2)
                return false;
        }

        return true;
    }

    public override string ToDebugString()
    {
        string s = "";
        foreach (var r in Requirements)
        {
            s += " Req: " + r.Item1 + " " + r.Item2;
        }

        s += base.ToDebugString();
        return s;
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

