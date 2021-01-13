using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadableItemHandler : MonoBehaviour
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
        string str = "New Order:\n";

        foreach (ItemAmountPair pair in order.Items)
        {
            str += ItemsData.GetItemInfo(pair.type).DisplayName + " x " + pair.amount + "\n";
        }

        foreach (UpgradeType upgrade in order.Upgrades)
        {
            str += upgrade.ToString() + "\n";
        }

        int index = currentIndex++;
        readableItems.Add(index, str);
        return index;
    }

    public int AddNewReadable(string str)
    {
        int index = currentIndex++;
        readableItems.Add(index, str);
        return index;
    }

    internal string GetAuthor(int id)
    {
        return LettersHolder.Instance.GetLetterWithID(id).Author;
    }
}
