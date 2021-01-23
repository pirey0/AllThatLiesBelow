using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReadableItemHandler : MonoBehaviour, ISavable
{
    const int START_INDEX = 10000;

    [SerializeField] Sprite iconClosed, iconOpen;
    [SerializeField] ReadableItemVisualizer textDisplayPrefab;
    [SerializeField] AudioSource ReadLetterSound;
    [SerializeField] Canvas canvas;

    Dictionary<int, string> readableItems = new Dictionary<int, string>();
    List<int> readLettersIds = new List<int>();
    int currentIndex = START_INDEX;
    ReadableItemVisualizer current;

    [Zenject.Inject] ReadableItemVisualizer.Factory readableItemFactory;

    public event System.Action HideEvent;

    public void Display(int id, InventorySlotVisualizer slotVisualizer)
    {
        if (current != null && current.id == id)
        {
            current.Hide();
        }
        else
        {
            if (current != null)
                current.Hide();

            current = readableItemFactory.Create(textDisplayPrefab);
            current.transform.SetParent(canvas.transform, worldPositionStays: false);
            current.id = id;
            ReadLetterSound.pitch = 1;
            ReadLetterSound.Play();
            string textToDisplay = null;

            if (readableItems.ContainsKey(id))
            {
                textToDisplay = readableItems[id];
            }
            else
            {
                var letterInfo = LettersHolder.Instance.GetLetterWithID(id);
                if (letterInfo != null)
                    textToDisplay = letterInfo.Content;
            }

            if (!readLettersIds.Contains(id))
            {
                readLettersIds.Add(id);
                slotVisualizer?.Refresh();
            }

            if (textToDisplay == null)
            {
                Debug.LogError("No letter found matching ID " + id);
            }
            else
            {
                current.DisplayText(transform, textToDisplay);
                Debug.Log("display letter with ID " + id);
            }
        }
    }

    public bool HasRead(int id)
    {
        return readLettersIds.Contains(id);
    }

    public void Hide()
    {
        if (current != null)
        {
            current.Hide();

            ReadLetterSound.pitch = 0.66f;
            ReadLetterSound.Play();
            HideEvent?.Invoke();
        }
    }

    public int AddNewOrder(Order order)
    {
        string str = "Hey Rick, please get me the following things: \n\n";

        foreach (ItemAmountPair pair in order.Items)
        {
            str += ItemsData.GetItemInfo(pair.type).DisplayName + " x " + pair.amount + "\n";
        }

        foreach (UpgradeType upgrade in order.Upgrades)
        {
            str += upgrade.ToString() + "\n";
        }

        str += "\nThe payment is attached to this list. \n\n - John";

        int index = FindMatchingReadable(str);
        if (index < 0) //if not found assign new index
        {
            index = currentIndex++;
            readableItems.Add(index, str);
        }
        return index;
    }

    public int AddNewReadable(string str)
    {
        int index = FindMatchingReadable(str);
        if (index < 0) //if not found assign new index
        {
            index = currentIndex++;
            readableItems.Add(index, str);
        }
        return index;
    }

    private int FindMatchingReadable(string str)
    {
        var vs = readableItems.Values.ToArray();

        for (int i = 0; i < vs.Length; i++)
        {
            if (vs[i] == str)
            {
                return i;
            }
        }
        return -1;
    }

    internal string GetAuthor(int id)
    {
        var letter = LettersHolder.Instance.GetLetterWithID(id);

        return letter == null ? null : letter.Author;
    }

    public SaveData ToSaveData()
    {
        var sd = new ReadableItemHandlerSaveData();
        sd.GUID = GetSaveID();
        sd.CurrentID = currentIndex;
        sd.ReadLettersIds = readLettersIds;
        sd.ReadableItems = readableItems;

        return sd;
    }

    public void Load(SaveData data)
    {
        if (data is ReadableItemHandlerSaveData sd)
        {
            readLettersIds = sd.ReadLettersIds;
            currentIndex = sd.CurrentID;
            readableItems = sd.ReadableItems;
        }
    }

    public string GetSaveID()
    {
        return "ReadableItemHandler";
    }

    public int GetLoadPriority()
    {
        return 0;
    }

    [System.Serializable]
    public class ReadableItemHandlerSaveData : SaveData
    {
        public List<int> ReadLettersIds;
        public int CurrentID;
        public Dictionary<int, string> ReadableItems;
    }
}
