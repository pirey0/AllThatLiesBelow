using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        TextAsset textAsset = Resources.Load<TextAsset>(PATH);
        if (textAsset == null)
        {
            Debug.LogError("Failed to load Dialogs");
            return;
        }

        string[] lines = textAsset.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        string[] descriptions = lines[0].Split(';');

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

    public string this[string s] { get => content.ContainsKey(s)? content[s] : ""; }

    public SVGDialogEntry(string line, string[] descriptions)
    {
        string[] elements = line.Split(';');
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
