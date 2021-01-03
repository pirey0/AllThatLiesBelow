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

        StringTreeCollection trees = board.FilterOutStringTreesWithRoot(log: true , "Dialog", "Choice");

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
            if(c is AltarDialogRootNode rootNode)
            {
                if(rootNode.Name == name)
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



    public StringTreeCollection FilterOutStringTreesWithRoot(bool log, params string[] acceptedRoots)
    {
        Dictionary<string, MiroTreeWidget> treeWidgets = new Dictionary<string, MiroTreeWidget>();

        //create nodes
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

        //Connect nodes
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

        //Remove all widgets that are not connected by Lines
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

        if (log)
            Debug.Log("TreeWidgets: " + treeWidgets.Count);

        //Find root widgets
        List<MiroTreeWidget> rootWidgets = new List<MiroTreeWidget>();
        foreach (var p in treeWidgets)
        {
            if (p.Value.parentsIds.Count == 0)
            {
                rootWidgets.Add(p.Value);
            }
        }

        if (log)
            Debug.Log("Root Widgets found: " + rootWidgets.Count);

        //Remove all that dont match accepted roots
        for (int i = 0; i < rootWidgets.Count; i++)
        {
            if (!Util.StringMatches(rootWidgets[i].plainText, acceptedRoots))
            {
                rootWidgets.RemoveAt(i);
                i--;
            }
        }

        if (log)
            Debug.Log("Accepted Root Widgets: " + rootWidgets.Count);

        //ToDo: Remove all that are not in the accepted roots


        //Convert to string tree
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

    [SerializeField]  private string[] keys;
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