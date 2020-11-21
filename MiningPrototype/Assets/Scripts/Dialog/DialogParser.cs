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
                Debug.Log("p: " + path + "x: " +x + " y:"+y);
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

public class DialogParser
{
    const string PATH = "DialogsData";

    static List<SVGDialogEntry> entries = new List<SVGDialogEntry>();
    static Dictionary<string, int> nameToIndexMap = new Dictionary<string, int>();
    static Dictionary<string, DialogConsequence> stringToConsequence = new Dictionary<string, DialogConsequence>()
    {
         {"", DialogConsequence.Null },
        {"Choice", DialogConsequence.Choice },
        {"Exit", DialogConsequence.Exit },
        {"JumpBeforeSentence", DialogConsequence.JumpBeforeSentence },
        {"JumpAfterSentence", DialogConsequence.JumpAfterSentence },
        {"AwaitPayment", DialogConsequence.AwaitPayment },
    };
    static Dictionary<string, DialogChoiceType> stringToType = new Dictionary<string, DialogChoiceType>()
    {
         {"", DialogChoiceType.Null },
        {"Sentence", DialogChoiceType.Sentence },
        {"Choice", DialogChoiceType.Choice },
        {"Entry", DialogChoiceType.Entry }
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ParseDialogs()
    {
        DurationTracker tracker = new DurationTracker("DialogParser");

        string[] lines = CSVHelper.LoadLinesAtPath(PATH);
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        string[] descriptions = CSVHelper.Split(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            var entry = new SVGDialogEntry(lines[i], descriptions);
            if (entry["SectionName"] != String.Empty)
            {
                nameToIndexMap.Add(entry["SectionName"], entries.Count);
                entries.Add(entry);

                entry.type = stringToType[entry["OptionType"]];
                entry.consequence = stringToConsequence[entry["Consequence"]];
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];

            if (nameToIndexMap.ContainsKey(e["JumpTarget"]))
            {
                e.jumpToTarget = entries[nameToIndexMap[e["JumpTarget"]]];
            }
            else
            {
                e.jumpToTarget = null;
            }

            IDialogSection[] sections = new IDialogSection[e.choices.Length];

            for (int j = 0; j < sections.Length; j++)
            {
                if (nameToIndexMap.ContainsKey(e.choices[j]))
                {
                    sections[j] = entries[nameToIndexMap[e.choices[j]]];
                }
                else
                {
                    Debug.LogError("Couldnt find: " + e.choices[j]);
                    sections[j] = null;
                }
            }
            e.choiceTargets = sections;
        }

        tracker.Stop();
    }

    public static SVGDialogEntry GetDialogFromName(string name)
    {
        if (nameToIndexMap.ContainsKey(name))
        {
            return entries[nameToIndexMap[name]];
        }
        return null;
    }

}

public class SVGDialogEntry : IDialogSection
{
    public Dictionary<string, string> content;

    public string[] choices;
    public DialogChoiceType type;
    public DialogConsequence consequence;
    public IDialogSection jumpToTarget;
    public IDialogSection[] choiceTargets;

    public string this[string s] { get => content.ContainsKey(s) ? content[s] : ""; }

    public SVGDialogEntry(string line, string[] descriptions)
    {
        string[] elements = CSVHelper.Split(line);
        content = new Dictionary<string, string>();

        List<string> tempChoices = new List<string>();
        //Filter choices out for comfort
        for (int i = 0; i < elements.Length; i++)
        {
            content.Add(descriptions[i], elements[i]);
            if (descriptions[i].Contains("Choice") && elements[i] != String.Empty)
            {
                tempChoices.Add(elements[i]);
            }
        }
        choices = tempChoices.ToArray();
    }


    public IDialogSection[] Choiches => choiceTargets;
    public IDialogSection JumpToTarget => jumpToTarget;
    public DialogChoiceType OptionType => type;
    public string OptionText => this["OptionText"];
    public string Sentence => this["Sentence"];
    public DialogConsequence Consequence => consequence;

    public string GetTopic()
    {
        return this["SetTopic"];
    }

    public string GetPayment()
    {
        return this["SetPayment"];
    }
}
