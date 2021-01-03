using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MiroParser
{
    const string path = "MiroBoard";

    public static void TestRun()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("Failed to load " + path);
            return;
        }

        string json = "{\"widgets\":" + textAsset.text + "}";
        var board = JsonUtility.FromJson<MiroBoard>(json);

        board.FilterOutUnnecessaryWidgets();
        Debug.Log("Widgets after Filter: " + board.widgets.Count);
        board.CreateTreeWidgets();
    }
}

[System.Serializable]
public class MiroBoard
{
    public List<MiroWidgetRaw> widgets;

    public void CreateTreeWidgets()
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
                    wEnd.children.Add(wStart);
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

        Debug.Log("TreeWidgets: " + treeWidgets.Count);
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
}


public class MiroTreeWidget
{
    public string plainText;

    public List<MiroTreeWidget> parents = new List<MiroTreeWidget>();
    public List<MiroTreeWidget> children = new List<MiroTreeWidget>();
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