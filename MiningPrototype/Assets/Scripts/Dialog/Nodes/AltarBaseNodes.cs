using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeResult
{
    Error = -2,
    Wait = -1,
    First = 0,
    Second = 1
}

public interface INodeServiceProvider
{
    IDialogVisualizer DialogVisualizer { get; }
    IDialogPropertiesHandler Properties { get; }
    AltarTreeCollection AltarTreeCollection { get; }

    IDialogInventoryHandler DialogInventoryHandler { get; }


}

public interface IDialogInventoryHandler
{
    bool InventoryConnected();

    Inventory GetConnectedInventory();
}

public interface IDialogPropertiesHandler
{
    bool GetVariable(string name);
    void SetVariable(string variableName, bool variableState);
    void FireEvent(string @event);
}

public interface IStartableNode
{
    NodeResult Start(INodeServiceProvider services);
}


public interface ITickingNode
{
    NodeResult Tick(INodeServiceProvider services);
}

public interface IEndableNode
{
    void OnEnd(INodeServiceProvider services);
}

public interface IConditionalNode
{
    bool ConditionPassed(INodeServiceProvider services);
}


[System.Serializable]
public class AltarBaseNode
{
    public AltarBaseNode[] Children;
    public string ID;

    public bool HasNext()
    {
        return Children != null && Children.Length > 0;
    }

    public virtual string ToDebugString()
    {
        if (Children == null || Children.Length == 0)
            return "";

        string s = " (";
        foreach (var item in Children)
        {
            s += item.ToDebugString();
            s += ", ";
        }
        s += ")";
        return s;
    }
}

public class AltarTreeCollection
{
    public AltarBaseNode[] Roots;
    public Dictionary<string, AltarBaseNode> Nodes;


    public AltarBaseNode[] GetEncounters()
    {
        List<AltarBaseNode> encounters = new List<AltarBaseNode>(Roots.Length);

        foreach (var item in Roots)
        {
            if(item is AltarDialogRootNode rootNode)
            {
                if (rootNode.IsEncounter)
                    encounters.Add(item);
            }
        }

        return encounters.ToArray();
    }

    public AltarDialogRootNode FindDialogWithName(string name)
    {
        foreach (var c in Roots)
        {
            if (c is AltarDialogRootNode rootNode)
            {
                if (rootNode.Name == name)
                {
                    return rootNode;
                }
            }
        }
        return null;
    }
}