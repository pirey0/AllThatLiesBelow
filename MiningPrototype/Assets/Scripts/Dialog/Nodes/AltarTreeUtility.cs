using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AltarTreeUtility
{
    public static AltarTreeCollection ConvertStringCollectionToAltarTree(StringTreeCollection collection)
    {
        AltarTreeCollection newCollection = new AltarTreeCollection();
        newCollection.Nodes = new Dictionary<string, AltarBaseNode>();

        foreach (var n in collection.Nodes)
        {
            var node = MakeBaseAltarNodeFromStringNode(n.Value);
            if (node != null)
                newCollection.Nodes.Add(n.Key, node);
        }

        foreach (var n in newCollection.Nodes)
        {
            SetDelayedReferences(n.Value, collection.Nodes[n.Key], newCollection, collection);
        }

        newCollection.Roots = new AltarBaseNode[collection.RootIds.Length];

        for (int i = 0; i < newCollection.Roots.Length; i++)
        {
            newCollection.Roots[i] = newCollection.Nodes[collection.RootIds[i]];
        }

        return newCollection;
    }

    public static AltarBaseNode MakeBaseAltarNodeFromStringNode(StringTreeNode root)
    {
        AltarBaseNode node = null;
        string m = root.text;
        string[] elements;

        if (NodeMatchesConditions(root, out elements, "Set", 3))
        {
            node = MakeAltarSetVariableNode(elements);
        }
        else if (NodeMatchesConditions(root, out elements, "Trigger", 2))
        {
            node = MakeAltarEventTriggerNode(elements);
        }
        else if (NodeMatchesConditions(root, out elements, "Dialog", 1))
        {
            node = new AltarDialogRootNode();
        }
        else if (NodeMatchesConditions(root, out elements, "Option", 1))
        {
            node = new AltarOptionNode();
        }
        else if (NodeMatchesConditions(root, out elements, "Choice", 1))
        {
            node = new AltarChoiceNode();
        }
        else if (NodeMatchesConditions(root, out elements, "Name", 2))
        {
            return null;
        }
        else if (NodeMatchesConditions(root, out elements, "Require", 2, 3))
        {
            return null;
        }
        else if (StringNodeMatchesConditions(root, out elements, "Text"))
        {
            return null;
        }
        else
        {
            var tnode = new AltarTextNode();
            tnode.Text = root.text;
            node = tnode;
        }

        node.ID = root.id;
        return node;
    }

    public static void SetDelayedReferences(AltarBaseNode node, StringTreeNode root, AltarTreeCollection collection, StringTreeCollection stringCollection)
    {
        List<AltarBaseNode> children = new List<AltarBaseNode>();

        for (int i = 0; i < root.childrenIds.Length; i++)
        {
            var key = root.childrenIds[i];
            if (collection.Nodes.ContainsKey(key))
            {
                var newN = collection.Nodes[key];
                children.Add(newN);
            }
        }


        node.Children = children.ToArray();

        if (node is AltarConditionalNode conditionalNode)
        {
            AddRequirementsToConditionalNode(root, conditionalNode, stringCollection);
        }


        if (node is AltarOptionNode optionNode)
        {
            string[] elements;

            foreach (var nodeId in root.childrenIds)
            {
                if (StringNodeMatchesConditions(stringCollection.Nodes[nodeId], out elements, "Text"))
                {
                    elements[0] = "";
                    optionNode.OptionText = string.Join(" ", elements);
                }
            }
        }
        else if (node is AltarDialogRootNode altarDialogRootNode)
        {
            string[] elements;

            foreach (var nodeId in root.childrenIds)
            {
                if ((NodeMatchesConditions(stringCollection.Nodes[nodeId], out elements, "Name", 2)))
                {
                    altarDialogRootNode.Name = elements[1];
                }
            }
        }



    }

    private static void AddRequirementsToConditionalNode(StringTreeNode stringNode, AltarConditionalNode node, StringTreeCollection stringCollection)
    {
        string[] elements;
        foreach (var nodeId in stringNode.childrenIds)
        {
            if ((NodeMatchesConditions(stringCollection.Nodes[nodeId], out elements, "Require", 2, 3)))
            {
                if (elements.Length == 2)
                {
                    node.Requirements.Add((elements[1], true));
                }
                else if (elements.Length == 3)
                {
                    if (Util.StringMatches(elements[1], "not", "Not"))
                    {
                        node.Requirements.Add((elements[2], false));
                    }
                }
            }
        }
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


    private static bool NodeMatchesConditions(StringTreeNode input, out string[] elements, string reqElement0, params int[] acceptedLength)
    {
        elements = input.text.Split(new string[] { Environment.NewLine, " " }, StringSplitOptions.RemoveEmptyEntries);
        return Util.Matches(elements.Length, acceptedLength) && elements[0] == reqElement0;
    }

    private static bool StringNodeMatchesConditions(StringTreeNode input, out string[] elements, string reqElement0)
    {
        elements = input.text.Split(new string[] { Environment.NewLine, " " }, StringSplitOptions.RemoveEmptyEntries);
        return elements[0] == reqElement0;
    }
}
