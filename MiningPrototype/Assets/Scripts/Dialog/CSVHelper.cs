using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public static class CSVHelper
{
    public static bool ResourceMissing(string path)
    {
        return Resources.Load(path) == null;
    }

    public static string[] LoadLinesAtPath(string path)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("Failed to load " + path);
            return null;
        }

        Regex regex = new Regex(@"^([^""\r\n]*(?:(?:""[^""]*"")*[^""\r\n]*))", RegexOptions.Multiline);
        MatchCollection matchCollection = regex.Matches(textAsset.text);
        List<string> results = new List<string>();
        for (int i = 0; i < matchCollection.Count; i++)
        {
            Match match = matchCollection[i];
            if (match.Value != "")
                results.Add(match.Value);
        }
        return results.ToArray();
    }

    public static string[,] LoadTableAtPath(string path)
    {
        string[] lines = LoadLinesAtPath(path);

        if (lines == null || lines.Length == 0)
            return null;

        int count = Split(lines[0]).Length;

        string[,] table = new string[count, lines.Length];

        for (int y = 0; y < table.GetLength(1); y++)
        {
            string[] current = Split(lines[y]);
            for (int x = 0; x < table.GetLength(0); x++)
            {
                table[x, y] = current[x];
            }
        }
        return table;
    }

    public static string[] Split(string s)
    {
        return s.Split(';');
    }

    public static string[] GetRow0(string path)
    {
        string[] lines = LoadLinesAtPath(path);
        if (lines == null || lines.Length == 0)
            return null;

        return Split(lines[0]);
    }

    public static string[] GetColumn0(string path)
    {
        string[] lines = LoadLinesAtPath(path);
        if (lines == null || lines.Length == 0)
            return null;

        string[] result = lines.Select((x) => Split(x)[0]).ToArray();
        return result;
    }
}
