using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LettersHolder
{
    const string PATH = "Letters/";

    public const string LETTERS_ID_DESC = "0-100 NarrativeLetters, 101-9999 Custom Letters , 10000- Orders"; 

    Dictionary<int, Letter> lettersTable;
    private static LettersHolder instance;

    public static LettersHolder Instance
    {
        get
        {
            if (instance == null)
                instance = new LettersHolder();

            return instance;
        }
    }

    private LettersHolder()
    {
        Refresh();
    }

    public void Refresh()
    {
        var letters = Resources.LoadAll<Letter>(PATH);
        lettersTable = new Dictionary<int, Letter>();
        foreach (var l in letters)
        {
            lettersTable.Add(l.ID, l);
        }
        Debug.Log("LettersParser: loaded " + letters.Length + " letters.");
    }

    public Letter GetLetterWithID(int id)
    {
        if (lettersTable.ContainsKey(id))
        {
            return lettersTable[id];
        }
        return null;
    }
}
