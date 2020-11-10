using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Section")]
public class ManualDialogSection : IDialogSection
{
    public string optionText;
    public DialogChoiceType optionType;

    public string sentence;

    public DialogConsequence consequence;

    public IDialogSection[] choiches;
    public IDialogSection jumpToTarget;

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
    JumpBeforeSentenceToPayment,
    JumpAfterSentenceToPayment
}