using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


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

        StringTreeNode trees = board.FilterOutStringTreesWithRoot(log: true , "Dialog", "Choice");

        string treesAsJson = JsonUtility.ToJson(trees);

        File.WriteAllText(Application.dataPath + localTreesAssetPath, treesAsJson);
        UnityEditor.AssetDatabase.Refresh();
    }

    public static void TestLoadAltarTrees()
    {
        var asset = Resources.Load<TextAsset>(resourcesTreesAssetPath);

        StringTreeNode root = JsonUtility.FromJson<StringTreeNode>(asset.text);

        AltarTreeNode res = AltarTreeUtility.MakeAltarTreeFromStringTree(root);
        Debug.Log(res.ToDebugString());
    }
#endif
}

[System.Serializable]
public class MiroBoard
{
    public List<MiroWidgetRaw> widgets;



    public StringTreeNode FilterOutStringTreesWithRoot(bool log, params string[] acceptedRoots)
    {
        Dictionary<string, MiroTreeWidget> treeWidgets = new Dictionary<string, MiroTreeWidget>();

        //create nodes
        foreach (var w in widgets)
        {
            if (w.type == "STICKER")
            {
                var nw = new MiroTreeWidget();
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
                    wStart.children.Add(wEnd);
                    wEnd.parents.Add(wStart);
                }
            }
        }

        //Remove all widgets that are not connected by Lines
        List<string> keysToRemove = new List<string>();
        foreach (var p in treeWidgets)
        {
            if (p.Value.children.Count == 0 && p.Value.parents.Count == 0)
            {
                keysToRemove.Add(p.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            treeWidgets.Remove(key);
        }

        if(log)
        Debug.Log("TreeWidgets: " + treeWidgets.Count);

        //Find root widgets
        List<MiroTreeWidget> rootWidgets = new List<MiroTreeWidget>();
        foreach (var p in treeWidgets)
        {
            if (p.Value.parents.Count == 0)
            {
                rootWidgets.Add(p.Value);
            }
        }

        if(log)
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

        if(log)
        Debug.Log("Accepted Root Widgets: " + rootWidgets.Count);

        //Check for circularities
        for (int i = 0; i < rootWidgets.Count; i++)
        {
            if(HasCircularities(rootWidgets[i], new List<MiroTreeWidget>(), out MiroTreeWidget culprit))
            {
                Debug.Log("Circularities in " + rootWidgets[i].plainText + " at " + culprit.plainText);
                rootWidgets.RemoveAt(i);
                i--;
            }
        }

        if (log)
        {
            foreach (var item in rootWidgets)
            {
                Debug.Log(item.plainText + " with a count of " + item.GetSubWidgetCount());
            }
        }
    

        //Convert to string tree
        StringTreeNode[] trees = new StringTreeNode[rootWidgets.Count];

        for (int i = 0; i < trees.Length; i++)
        {
            trees[i] = Rec_MiroTreeToStringTree(rootWidgets[i]);
            if (log)
                Debug.Log(trees[i].ToDebugString());
        }

        StringTreeNode root = new StringTreeNode();
        root.text = "ROOT";
        root.children = trees;
        return root;
    }

    public bool HasCircularities(MiroTreeWidget widget, List<MiroTreeWidget> covered, out MiroTreeWidget culprit)
    {
        if (covered.Contains(widget))
        {
            culprit = widget;
            return true;
        }
        else
        {
            covered.Add(widget);
            foreach (var item in widget.children)
            {
               if( HasCircularities(item, covered, out culprit))
                {
                    return true;
                }
            }
        }

        culprit = null;
        return false;
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


    public StringTreeNode Rec_MiroTreeToStringTree(MiroTreeWidget root)
    {
        StringTreeNode node = new StringTreeNode();
        node.text = root.plainText;

        if (root.children.Count > 0)
        {
            node.children = new StringTreeNode[root.children.Count];

            for (int i = 0; i < node.children.Length; i++)
            {
                node.children[i] = Rec_MiroTreeToStringTree(root.children[i]);
            }
        }
        return node;
    }
}


[System.Serializable]
public class StringTreeNode
{
    public string text;
    public StringTreeNode[] children;


    public string ToDebugString()
    {
        string s = text;

        if (children != null)
        {
            s += "(";
            foreach (var item in children)
            {
                s += item.ToDebugString();
                s += ", ";
            }
            s += ")";
        }
        return s;
    }
}


public class MiroTreeWidget
{
    public string plainText;

    public List<MiroTreeWidget> parents = new List<MiroTreeWidget>();
    public List<MiroTreeWidget> children = new List<MiroTreeWidget>();

    public int GetSubWidgetCount()
    {
        int count = children.Count;
        foreach (var item in children)
        {
            count += item.GetSubWidgetCount();
        }
        return count;
    }
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