using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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