using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Section")]
public class DialogSection : ScriptableObject, IDialogSection
{
    public string optionText;
    public DialogChoiceType optionType;

    [TextArea(2, 10)]
    public string sentence;

    public DialogConsequence consequence;

    public DialogSection[] choiches;
    public DialogSection jumpToTarget;

    public string OptionText => optionText;
    public DialogChoiceType OptionType => optionType;
    public string Sentence => sentence;
    public DialogConsequence Consequence => consequence;
    public IDialogSection[] Choiches => choiches;
    public IDialogSection JumpToTarget => jumpToTarget;
}

public interface IDialogSection
{
    string OptionText { get; }
    DialogChoiceType OptionType { get; }

    string Sentence { get; }

    DialogConsequence Consequence { get; }

    IDialogSection[] Choiches { get; }
    IDialogSection JumpToTarget { get; }
}


public enum DialogChoiceType
{
    Null,
    Sentence,
    Choice,
    Entry
}

public enum DialogConsequence
{
    Null,
    Choice,
    Exit,
    JumpBeforeSentence,
    JumpAfterSentence,
    AwaitPayment
}