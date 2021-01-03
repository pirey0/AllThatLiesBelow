using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AltarTreeUtility
{
    public static AltarTreeNode MakeAltarTreeFromStringTree(StringTreeNode root)
    {
        AltarTreeNode node = null;
        string m = root.text;
        string[] elements;

        if (StringNodeMatchesConditions(root, out elements, "Set", 3))
        {
            node = MakeAltarSetVariableNode(elements);
        }
        else if (StringNodeMatchesConditions(root, out elements, "Trigger", 2))
        {
            node = MakeAltarEventTriggerNode(elements);
        }
        else if (StringNodeMatchesConditions(root, out elements, "Dialog", 1))
        {
            node = MakeTreeRootNode(root);
        }
        else if (StringNodeMatchesConditions(root, out elements, "Choice", 1))
        {
            node = MakeTreeRootNode(root);
        }
        else if (StringNodeMatchesConditions(root, out elements, "Name", 2))
        {
            return null;
        }
        else if (StringNodeMatchesConditions(root, out elements, "Require", 2, 3))
        {
            return null;
        }
        else
        {
            node = MakeTextNode(root.text);
        }

        List<AltarTreeNode> children = new List<AltarTreeNode>();

        for (int i = 0; i < root.children.Length; i++)
        {
            var newN = MakeAltarTreeFromStringTree(root.children[i]);
            if (newN != null)
                children.Add(newN);
        }

        node.Children = children.ToArray();
        return node;
    }

    private static AltarTextNode MakeTextNode(string text)
    {
        var n = new AltarTextNode();
        n.Text = text;
        return n;
    }

    private static AltarTreeRootNode MakeTreeRootNode(StringTreeNode node)
    {
        AltarTreeRootNode rootNode = new AltarTreeRootNode();
        rootNode.Type = node.text;
        string[] elements;

        foreach (var cNode in node.children)
        {
            if ((StringNodeMatchesConditions(cNode, out elements, "Name", 2)))
            {
                rootNode.Name = elements[1];
            }
            else if ((StringNodeMatchesConditions(cNode, out elements, "Require", 2, 3)))
            {
                if (elements.Length == 2)
                {
                    rootNode.Requirements.Add((elements[1], true));
                }
                else if (elements.Length == 3)
                {
                    if (Util.StringMatches(elements[1], "not", "Not"))
                    {
                        rootNode.Requirements.Add((elements[2], false));
                    }
                }
            }
        }

        return rootNode;
    }


    private static AltarSetVariableNode MakeAltarSetVariableNode(string[] elements)
    {
        if (bool.TryParse(elements[2], out bool res))
        {
            AltarSetVariableNode set = new AltarSetVariableNode();
            set.VariableName = elements[1];
            set.VariableState = res;
            return set;
        }
        return null;
    }

    private static AltarEventTriggerNode MakeAltarEventTriggerNode(string[] elements)
    {
        AltarEventTriggerNode node = new AltarEventTriggerNode();
        node.Event = elements[1];
        return node;
    }


    private static bool StringNodeMatchesConditions(StringTreeNode input, out string[] elements, string reqElement0, params int[] acceptedLength)
    {
        elements = input.text.Split(new string[] { Environment.NewLine, " " }, StringSplitOptions.RemoveEmptyEntries);
        return Util.Matches(elements.Length, acceptedLength) && elements[0] == reqElement0;
    }
}


[System.Serializable]
public class AltarTreeRootNode : AltarTreeNode
{
    public string Type;
    public string Name;

    public List<(string, bool)> Requirements = new List<(string, bool)>();

    public override string ToDebugString()
    {
        string s = "Root: " + Type + " Name: " + Name;

        foreach (var r in Requirements)
        {
            s += " Req: " + r.Item1 + " " + r.Item2;
        }

        s += base.ToDebugString();
        return s;
    }
}

[System.Serializable]
public class AltarTextNode : AltarTreeNode
{
    public string Text;

    public override string ToDebugString()
    {
        return "\"" + Text + "\"" + base.ToDebugString();
    }
}

[System.Serializable]
public class AltarSetVariableNode : AltarTreeNode
{
    public string VariableName;
    public bool VariableState;

    public override string ToDebugString()
    {
        return "Set: " + VariableName + ": " + VariableState + base.ToDebugString();
    }

}

[System.Serializable]
public class AltarEventTriggerNode : AltarTreeNode
{
    public string Event;

    public override string ToDebugString()
    {
        return "Event: " + Event + base.ToDebugString();
    }
}

[System.Serializable]
public class AltarTreeNode
{
    public AltarTreeNode[] Children;

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