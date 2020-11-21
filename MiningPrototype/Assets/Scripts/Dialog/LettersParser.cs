using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LettersParser : MonoBehaviour
{
    const string PATH = "LettersData";

    static Dictionary<int, LetterInfo> lettersTable;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ParseLetters()
    {
        DurationTracker tracker = new DurationTracker("LetterParser");

        if (CSVHelper.ResourceMissing(PATH))
            return;

        lettersTable = new Dictionary<int, LetterInfo>();
        string[,] table = CSVHelper.LoadTableAtPath(PATH);

        for (int y = 1; y < table.GetLength(1); y++)
        {
            LetterInfo info = new LetterInfo();

            if (int.TryParse(table[0, y], out int res))
            {
                info.Id = res;
            }
            else
            {
                Debug.LogError("No id for letter at y: " + y);
                continue;
            }

            if (int.TryParse(table[2, y], out res))
            {
                info.AnswerId = res;
            }

            if (int.TryParse(table[3, y], out res))
            {
                info.IgnoreId = res;
            }

            info.Author = table[1, y];
            info.Content = table[4, y];

            if (!lettersTable.ContainsKey(info.Id))
            {
                lettersTable.Add(info.Id, info);
            }
            else
            {
                Debug.LogError("Double definition for letterID " + info.Id);
            }
        }

        Debug.Log("Loaded " + lettersTable.Keys.Count + " letters.");
        tracker.Stop();
    }

    public static LetterInfo GetLetterWithID(int id)
    {
        if (lettersTable.ContainsKey(id))
        {
            return lettersTable[id];
        }
        return null;
    }

    public class LetterInfo
    {
        public int Id;
        public string Author;

        public int AnswerId;
        public int IgnoreId;

        public string Content;
    }
}
