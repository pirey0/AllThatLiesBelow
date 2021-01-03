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

        for (int i = 0; i < board.widgets.Count; i++)
        {
            MiroWidget current = board.widgets[i];
            //Remove all but Sticker and Line
            string t = current.type;
            if (t != "STICKER" && t != "LINE")
            {
                board.widgets.RemoveAt(i);
                i--;
            }
            else if (t == "STICKER")
            {
                //Remove empty stickers
                if (string.IsNullOrWhiteSpace(current.plainText))
                {
                    board.widgets.RemoveAt(i);
                    i--;
                }
            }
            else if (t == "LINE")
            {
                //Remove lines that are not connected
                if (string.IsNullOrEmpty(current.endWidgetId) || string.IsNullOrEmpty(current.startWidgetId))
                {
                    board.widgets.RemoveAt(i);
                    i--;
                }
            }
        }
        Debug.Log(board.widgets.Count);
    }
}

[System.Serializable]
public class MiroBoard
{
    public List<MiroWidget> widgets;
}

[System.Serializable]
public class MiroWidget
{
    public string type;
    public string id;

    public string startWidgetId;
    public string endWidgetId;

    public string plainText;
}