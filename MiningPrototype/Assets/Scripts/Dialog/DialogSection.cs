using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Obsolete]
public interface IDialogSection
{
    string OptionText { get; }
    DialogChoiceType OptionType { get; }
    string Sentence { get; }
    DialogConsequence Consequence { get; }
    IDialogSection[] Choiches { get; }
    IDialogSection JumpToTarget { get; }
    string GetTopic();
    string GetPayment();
}

[System.Obsolete]
public enum DialogChoiceType
{
    Null,
    Sentence,
    Choice,
    Entry
}
[System.Obsolete]
public enum DialogConsequence
{
    Null,
    Choice,
    Exit,
    JumpBeforeSentence,
    JumpAfterSentence,
    AwaitPayment
}