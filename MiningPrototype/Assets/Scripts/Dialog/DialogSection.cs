using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Section")]
public class DialogSection : ScriptableObject
{
    public string OptionText;
    public DialogChoiceType OptionType;

    [TextArea(2, 10)]
    public string Sentence;

    public DialogConsequence Consequence;

    public DialogSection[] Choiches;
    public DialogSection JumpToTarget;

}


public enum DialogChoiceType
{
    Sentence,
    Choice,
    Entry
}

public enum DialogConsequence
{
    Choice,
    Exit,
    JumpBeforeSentence,
    JumpAfterSentence,
    AwaitPayment
}