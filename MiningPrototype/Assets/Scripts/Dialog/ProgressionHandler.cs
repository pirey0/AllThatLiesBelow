using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionHandler : Singleton<ProgressionHandler>
{
    [SerializeField] string dialog;
    public IDialogSection GetCurrentAltarDialog()
    {
        return DialogParser.GetDialogFromName(dialog);
    }
}
