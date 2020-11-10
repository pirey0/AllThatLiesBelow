using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogParser
{
    const string PATH = "DialogsData";
    const int SIZE = 14;

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


        for (int i = 1; i < lines.Length; i++)
        {
            if (!LineIsValid(lines[i]))
            {
                Debug.LogError("Invalid line found: " + lines[i]);
            }
            else
            {
                var entry = new SVGDialogEntry(lines[i]);
                if (entry.Name != String.Empty)
                {
                    nameToIndexMap.Add(entry.Name, entries.Count);
                    entries.Add(entry);
                }
                try
                {
                    entry.type = stringToType[entry.optionTypeText];
                    entry.consequence = stringToConsequence[entry.consequenceText];
                }
                catch
                {
                    Debug.LogError("Unknown: " + entry.optionTypeText + " or " + entry.consequenceText);
                }
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];

            if (nameToIndexMap.ContainsKey(e.jumpTargetText))
            {
                e.jumpToTarget = entries[nameToIndexMap[e.jumpTargetText]];
            }
            else
            {
                e.jumpToTarget = null;
            }

            IDialogSection[] sections = new IDialogSection[e.choicesText.Length];

            for (int j = 0; j < sections.Length; j++)
            {
                if (nameToIndexMap.ContainsKey(e.choicesText[j]))
                {
                    sections[j] = entries[nameToIndexMap[e.choicesText[j]]];
                }
                else
                {
                    Debug.LogError("Couldnt find: " + e.choicesText[j]);
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

    private static bool LineIsValid(string line)
    {
        var elements = line.Split(';');
        if (elements.Length != SIZE)
            return false;


        return true;
    }
}

public class SVGDialogEntry : IDialogSection
{
    public string Name;
    public string optionText;
    public string optionTypeText;
    public string sentenceText;
    public string consequenceText;
    public string jumpTargetText;
    public string[] choicesText;

    public DialogChoiceType type;
    public DialogConsequence consequence;
    public IDialogSection jumpToTarget;
    public IDialogSection[] choiceTargets;

    public SVGDialogEntry(string line)
    {
        string[] elements = line.Split(';');

        Name = elements[0];
        optionText = elements[1];
        optionTypeText = elements[2];
        sentenceText = elements[3];
        consequenceText = elements[4];
        jumpTargetText = elements[5];

        var list = new List<String>();

        for (int i = 6; i < elements.Length; i++)
        {
            if (elements[i] != String.Empty)
                list.Add(elements[i]);
        }
        choicesText = list.ToArray();
    }

    public IDialogSection[] Choiches => choiceTargets;
    public IDialogSection JumpToTarget => jumpToTarget;
    public DialogChoiceType OptionType => type;
    public string OptionText => optionText;
    public string Sentence => sentenceText;
    public DialogConsequence Consequence => consequence;
}
