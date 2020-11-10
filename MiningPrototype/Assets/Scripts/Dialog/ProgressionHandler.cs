using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionHandler : Singleton<ProgressionHandler>
{
    Dictionary<(string, string), float> priceMapping = new Dictionary<(string, string), float>()
    {
        {("Wealth", "Copper"), 100 },
        {("Wealth", "Gold"), 15 },
        {("Strength", "Copper"), 20 },
        {("Strength", "Gold"), 3 },
        {("Speed", "Copper"), 30 },
        {("Speed", "Gold"), 5 }
    };

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

    public float GetPriceOf(string topic, string payment)
    {
        if (priceMapping.ContainsKey((topic, payment)))
        {
            return priceMapping[(topic, payment)];
        }
        else
        {
            Debug.Log("Updefined price for " + topic + " and " + payment);
            return -1;
        }
    }
}
