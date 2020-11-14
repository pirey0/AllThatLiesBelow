using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDropReceiver
{
    bool WouldTakeDrop(ItemAmountPair pair);
    void BeginHoverWith(ItemAmountPair pair);
    void EndHover();
    void HoverUpdate(ItemAmountPair pair);
    void ReceiveDrop(ItemAmountPair pair);
}

public class Altar : MonoBehaviour, IInteractable, IDropReceiver
{
    [SerializeField] Transform cameraTarget;
    [SerializeField] AltarDialogVisualizer visualizer;
    [SerializeField] string testDialog;
    [SerializeField] SpriteRenderer overlay;

    bool inInteraction = false;
    DialogIterator iterator;
    private event System.Action NotifyForcedEnd;

    public void BeginInteracting(GameObject interactor)
    {
        inInteraction = true;
        visualizer.StartDialog();
        CameraController.Instance.TransitionToNewTarget(cameraTarget);

        IDialogSection dialog = testDialog == "" ? ProgressionHandler.Instance.GetCurrentAltarDialog() : DialogParser.GetDialogFromName(testDialog);
        iterator = new DialogIterator(dialog);
        iterator.StateChanged += OnStateChanged;
        visualizer.Progressed += OnProgressed;
        OnStateChanged();
    }

    private void OnProgressed(int index)
    {
        if (!inInteraction)
            return;

        switch (iterator.State)
        {
            case DialogState.Answer:
                iterator.Next();
                break;

            case DialogState.Choice:
                iterator.Select(index);
                break;
        }
    }

    private void OnStateChanged()
    {
        if (!inInteraction)
            return;

        if (iterator.CurrentSection == null)
        {
            NotifyForcedEnd?.Invoke();
            return;
        }

        switch (iterator.State)
        {
            case DialogState.Answer:
                visualizer.DisplaySentence(iterator.GetCorrectedSentence());
                break;

            case DialogState.Choice:
                var choices = iterator.CurrentSection.Choiches;
                string[] names = new string[choices.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = choices[i].OptionText;
                }
                visualizer.DisplayOptions(names);
                break;

            case DialogState.AwaitPayment:
                Debug.Log("Awaiting Payment");
                break;
        }
    }

    public void EndInteracting(GameObject interactor)
    {
        Debug.Log("End Altar Interaction");
        iterator.StateChanged -= OnStateChanged;
        visualizer.Progressed -= OnProgressed;
        visualizer.EndDialog();
        CameraController.Instance.TransitionToDefault();
        inInteraction = false;
    }

    public void SubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd += action;
    }

    public void UnsubscribeToForceQuit(Action action)
    {
        NotifyForcedEnd -= action;
    }

    public bool WouldTakeDrop(ItemAmountPair pair)
    {
        if (iterator == null)
            return false;

        return iterator.WouldAcceptPaymentWith(pair);
    }

    public void BeginHoverWith(ItemAmountPair pair)
    {
        Debug.Log("Begin hover with: " + pair.type);
        overlay.enabled = true;
    }

    public void EndHover()
    {
        overlay.enabled = false;
        Debug.Log("End Hover");
    }

    public void ReceiveDrop(ItemAmountPair pair)
    {
        Debug.Log("Try receive drop of " + pair.type);

        if (iterator.WouldAcceptPaymentWith(pair))
            iterator.PayWith(pair);
    }

    public void HoverUpdate(ItemAmountPair pair)
    {
        if (WouldTakeDrop(pair))
        {
            overlay.color = Color.green;
        }
        else
        {
            overlay.color = Color.red;
        }
    }
}
