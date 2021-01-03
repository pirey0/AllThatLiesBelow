using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class MiroParser
{
    const string resourcesTreesAssetPath = "StringTrees";
    const string localTreesAssetPath = "/Resources/" + resourcesTreesAssetPath + ".json";



#if UNITY_EDITOR
    public static void UpdateStringTreesFromMiroJsonFile(string miroPath)
    {
        if (!File.Exists(miroPath))
        {
            Debug.LogError("Invalid file");
            return;
        }

        string text = File.ReadAllText(miroPath);

        string json = "{\"widgets\":" + text + "}";
        var board = JsonUtility.FromJson<MiroBoard>(json);

        board.FilterOutUnnecessaryWidgets();
        Debug.Log("Widgets after Filter: " + board.widgets.Count);

        StringTreeCollection trees = board.FilterOutStringTrees(log: true, "Dialog", "Choice");

        trees.PrepareForSerialization();
        string treesAsJson = JsonUtility.ToJson(trees);

        File.WriteAllText(Application.dataPath + localTreesAssetPath, treesAsJson);
        UnityEditor.AssetDatabase.Refresh();
    }
#endif

    public static AltarBaseNode GetTestAltarDialogWithName(string name)
    {
        var asset = Resources.Load<TextAsset>(resourcesTreesAssetPath);

        StringTreeCollection root = JsonUtility.FromJson<StringTreeCollection>(asset.text);
        root.CleanupAfterDeserialization();
        AltarTreeCollection res = AltarTreeUtility.ConvertStringCollectionToAltarTree(root);

        foreach (var c in res.Roots)
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

[System.Serializable]
public class MiroBoard
{
    public List<MiroWidgetRaw> widgets;

    public StringTreeCollection FilterOutStringTrees(bool log, params string[] acceptedRoots)
    {
        Dictionary<string, MiroTreeWidget> treeWidgets = new Dictionary<string, MiroTreeWidget>();
        CreateNodes(treeWidgets);
        ConnectNodes(treeWidgets);
        RemoveUnconnectedWidgets(treeWidgets);

        if (log)
            Debug.Log("TreeWidgets: " + treeWidgets.Count);

        List<MiroTreeWidget> rootWidgets = FindRootWidgets(treeWidgets);

        if (log)
            Debug.Log("Root Widgets found: " + rootWidgets.Count);

        RemoveNonMatchingRoots(acceptedRoots, rootWidgets);

        if (log)
            Debug.Log("Accepted Root Widgets: " + rootWidgets.Count);

        RemoveNonCoveredWidgets(treeWidgets, rootWidgets);

        StringTreeCollection collection = ConvertToStringTreeCollection(treeWidgets, rootWidgets);

        SimplifyIds(collection);

        return collection;
    }

    private void SimplifyIds(StringTreeCollection collection)
    {
        int currId = 0;
        Dictionary<string, string> oldToNewMap = new Dictionary<string, string>();
        Dictionary<string, StringTreeNode> newDict = new Dictionary<string, StringTreeNode>();

        //remap keys
        foreach (var item in collection.Nodes)
        {
            string newId = (++currId).ToString();
            oldToNewMap.Add(item.Key, newId);
            newDict.Add(newId, item.Value);
            item.Value.id = newId;
        }

        //change rootsids
        for (int i = 0; i < collection.RootIds.Length; i++)
        {
            collection.RootIds[i] = oldToNewMap[collection.RootIds[i]];
        }

        //set to new dict
        collection.Nodes = newDict;

        //change childsids
        foreach (var item in collection.Nodes)
        {
            for (int i = 0; i < item.Value.childrenIds.Length; i++)
            {
                item.Value.childrenIds[i] = oldToNewMap[item.Value.childrenIds[i]];
            }
        }
    }

    private static List<MiroTreeWidget> FindRootWidgets(Dictionary<string, MiroTreeWidget> treeWidgets)
    {
        List<MiroTreeWidget> rootWidgets = new List<MiroTreeWidget>();
        foreach (var p in treeWidgets)
        {
            if (p.Value.parentsIds.Count == 0)
            {
                rootWidgets.Add(p.Value);
            }
        }

        return rootWidgets;
    }

    private StringTreeCollection ConvertToStringTreeCollection(Dictionary<string, MiroTreeWidget> treeWidgets, List<MiroTreeWidget> rootWidgets)
    {
        Dictionary<string, StringTreeNode> stringTrees = new Dictionary<string, StringTreeNode>();
        string[] roots = new string[rootWidgets.Count];

        foreach (var item in treeWidgets)
        {
            stringTrees.Add(item.Key, MiroTreeToStringTree(item.Value));
        }

        for (int i = 0; i < roots.Length; i++)
        {
            roots[i] = rootWidgets[i].id;
        }

        StringTreeCollection collection = new StringTreeCollection();
        collection.Nodes = stringTrees;
        collection.RootIds = roots;
        return collection;
    }

    private void RemoveNonCoveredWidgets(Dictionary<string, MiroTreeWidget> treeWidgets, List<MiroTreeWidget> rootWidgets)
    {
        List<string> coveredIDs = new List<string>();
        foreach (var item in rootWidgets)
        {
            RecAddIdToList(treeWidgets, coveredIDs, item);
        }
        List<string> keys = treeWidgets.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            if (!coveredIDs.Contains(keys[i]))
            {
                treeWidgets.Remove(keys[i]);
            }
        }
    }

    private static void RemoveNonMatchingRoots(string[] acceptedRoots, List<MiroTreeWidget> rootWidgets)
    {
        for (int i = 0; i < rootWidgets.Count; i++)
        {
            if (!Util.StringMatches(rootWidgets[i].plainText, acceptedRoots))
            {
                rootWidgets.RemoveAt(i);
                i--;
            }
        }
    }

    private static void RemoveUnconnectedWidgets(Dictionary<string, MiroTreeWidget> treeWidgets)
    {
        List<string> keysToRemove = new List<string>();
        foreach (var p in treeWidgets)
        {
            if (p.Value.childrenIds.Count == 0 && p.Value.parentsIds.Count == 0)
            {
                keysToRemove.Add(p.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            treeWidgets.Remove(key);
        }
    }

    private void ConnectNodes(Dictionary<string, MiroTreeWidget> treeWidgets)
    {
        foreach (var w in widgets)
        {
            if (w.type == "LINE")
            {
                if (treeWidgets.ContainsKey(w.startWidgetId) && treeWidgets.ContainsKey(w.endWidgetId))
                {
                    var wStart = treeWidgets[w.startWidgetId];
                    var wEnd = treeWidgets[w.endWidgetId];
                    wStart.childrenIds.Add(w.endWidgetId);
                    wEnd.parentsIds.Add(w.startWidgetId);
                }
            }
        }
    }

    private void CreateNodes(Dictionary<string, MiroTreeWidget> treeWidgets)
    {
        foreach (var w in widgets)
        {
            if (w.type == "STICKER")
            {
                var nw = new MiroTreeWidget();
                nw.id = w.id;
                nw.plainText = w.plainText;
                treeWidgets.Add(w.id, nw);
            }
        }
    }

    private void RecAddIdToList(Dictionary<string, MiroTreeWidget> nodes, List<string> list, MiroTreeWidget node)
    {
        list.Add(node.id);
        foreach (var id in node.childrenIds)
        {
            if (!list.Contains(id))
                RecAddIdToList(nodes, list, nodes[id]);
        }
    }

    public void FilterOutUnnecessaryWidgets()
    {
        for (int i = 0; i < widgets.Count; i++)
        {
            MiroWidgetRaw current = widgets[i];
            //Remove all but Sticker and Line
            string t = current.type;
            if (t != "STICKER" && t != "LINE")
            {
                widgets.RemoveAt(i);
                i--;
            }
            else if (t == "STICKER")
            {
                //Remove empty stickers
                if (string.IsNullOrWhiteSpace(current.plainText))
                {
                    widgets.RemoveAt(i);
                    i--;
                }
            }
            else if (t == "LINE")
            {
                //Remove lines that are not connected
                if (string.IsNullOrEmpty(current.endWidgetId) || string.IsNullOrEmpty(current.startWidgetId))
                {
                    widgets.RemoveAt(i);
                    i--;
                }
            }
        }
    }


    public StringTreeNode MiroTreeToStringTree(MiroTreeWidget root)
    {
        StringTreeNode node = new StringTreeNode();
        node.text = root.plainText;
        node.id = root.id;
        node.childrenIds = root.childrenIds.ToArray();
        return node;
    }
}

[System.Serializable]
public class StringTreeCollection
{
    public string[] RootIds;

    [System.NonSerialized] public Dictionary<string, StringTreeNode> Nodes;

    [SerializeField] private string[] keys;
    [SerializeField] private StringTreeNode[] values;

    public void PrepareForSerialization()
    {
        keys = Nodes.Keys.ToArray();
        values = Nodes.Values.ToArray();
    }

    public void CleanupAfterDeserialization()
    {
        Nodes = new Dictionary<string, StringTreeNode>();
        for (int i = 0; i < keys.Length; i++)
        {
            Nodes.Add(keys[i], values[i]);
        }
    }
}


[System.Serializable]
public class StringTreeNode
{
    public string id;
    public string text;
    public string[] childrenIds;


    public string ToDebugString()
    {
        string s = text;

        if (childrenIds != null)
        {
            s += "(";
            foreach (var item in childrenIds)
            {
                s += item + ", ";
            }
            s += ")";
        }
        return s;
    }
}


public class MiroTreeWidget
{
    public string id;
    public string plainText;

    public List<string> parentsIds = new List<string>();
    public List<string> childrenIds = new List<string>();

}

[System.Serializable]
public class MiroWidgetRaw
{
    public string type;
    public string id;

    public string startWidgetId;
    public string endWidgetId;

    public string plainText;
}