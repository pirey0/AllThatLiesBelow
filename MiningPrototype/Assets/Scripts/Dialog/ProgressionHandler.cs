using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionHandler : Singleton<ProgressionHandler>
{
    [SerializeField] string dialog;
    [SerializeField] string alreadyTradedDialog;

    bool dailyPurchaseExaused;

    public IDialogSection GetCurrentAltarDialog()
    {
        var di = DialogParser.GetDialogFromName(dailyPurchaseExaused ? alreadyTradedDialog : dialog);

        return di;
    }

    public void Aquired(string topic)
    {
        Debug.Log(topic + " unlocked in the morning! (Not implemented yet!");
        dailyPurchaseExaused = true;
    }

    public float GetPriceOf(string reward, string resource)
    {
        return PricesParser.GetPriceFor(reward, resource);
    }
}
