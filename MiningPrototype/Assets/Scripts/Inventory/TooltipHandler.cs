using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipHandler : Singleton<TooltipHandler>
{
    [SerializeField] TMPro.TMP_Text textUI, subTextUI;
    [SerializeField] Image box;

    Transform currentTarget;

    private void Update()
    {
        transform.position = Util.MouseToWorld();
    }

    public void Display(Transform target, string text, string subText)
    {
        currentTarget = target;
        textUI.text = text;
        subTextUI.text = subText;
        box.enabled = true;
     }

    public void StopDisplaying(Transform transform)
    {
        if(currentTarget = transform)
        {
            currentTarget = null;
            textUI.text = "";
            subTextUI.text = "";
            box.enabled = false;
        }
    }

}
