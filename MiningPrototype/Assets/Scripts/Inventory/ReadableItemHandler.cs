using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadableItemHandler : MonoBehaviour
{
    [SerializeField] Sprite iconClosed, iconOpen;
    [SerializeField] ReadableItemVisualizer textDisplayPrefab;
    [SerializeField] AudioSource ReadLetterSound;
    ReadableItemVisualizer current;
    [SerializeField] Canvas canvas;
    List<ReadableItem> readableItems = new List<ReadableItem>();
    List<int> readLettersIds = new List<int>();

    [Zenject.Inject] ReadableItemVisualizer.Factory readableItemFactory;

    public event System.Action HideEvent;

    public void Display(int id, InventorySlotVisualizer slotVisualizer)
    {
        if (current != null && current.id == id)
            current.Hide();
        else
        {
            if (current != null)
                current.Hide();

            current = readableItemFactory.Create(textDisplayPrefab);
            current.transform.SetParent(canvas.transform, worldPositionStays: false);
            current.id = id;
            ReadLetterSound.pitch = 1;
            ReadLetterSound.Play();
            ReadableItem itemToDisplay = (readableItems.Count - 1 < id) ? null : readableItems[id];

            if (itemToDisplay == null)
            {
                var letterInfo = LettersHolder.Instance.GetLetterWithID(id);
                if (letterInfo != null)
                    itemToDisplay = new ReadableItem(letterInfo.Content);
            }
          
            if (!readLettersIds.Contains(id))
            {
                readLettersIds.Add(id);
                slotVisualizer?.Refresh();
            }

            if (itemToDisplay == null)
            {
                Debug.LogError("No letter found matching ID " + id);
                return;
            }

            current.DisplayText(transform, itemToDisplay);
            Debug.Log("display letter with id:" + id);
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

    public int AddNewReadable(Order order)
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

        readableItems.Add(new ReadableItem(str, ItemType.NewOrder));
        return readableItems.Count - 1;
    }

    public int AddNewReadable(string str)
    {
        readableItems.Add(new ReadableItem(str));
        return readableItems.Count - 1;
    }
}

public class ReadableItem
{
    public ItemType itemType;
    public string text;

    public ReadableItem(string _text, ItemType _itemType = ItemType.LetterFromFamily)
    {
        text = _text;
        itemType = _itemType;
    }
}