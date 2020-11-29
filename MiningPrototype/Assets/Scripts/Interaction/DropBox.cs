using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DropBox : HoverHighlighter, IDropReceiver
{
    [SerializeField] ItemType[] sendable;
    [SerializeField] AudioSource storeItemAudio;

    [SerializeField] Queue<ItemAmountPair> storedItems = new Queue<ItemAmountPair>();
    public void BeginHoverWith(ItemAmountPair pair)
    {
        //
    }

    public void EndHover()
    {
        //
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        //
    }

    public void ReceiveDrop(ItemAmountPair pair, Inventory origin)
    {
        if (WouldTakeDrop(pair) && origin.Contains(pair) && origin.TryRemove(pair))
        {
     
            storedItems.Enqueue(pair);
            storeItemAudio?.Play();
        }

    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        return sendable.Contains(pair.type);
    }

    public ItemAmountPair FetchItem()
    {
        return storedItems.Dequeue();
    }

    public bool IsEmpty()
    {
        return storedItems.Count == 0;
    }
}
