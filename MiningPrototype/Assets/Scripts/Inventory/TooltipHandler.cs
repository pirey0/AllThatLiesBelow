using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipHandler : Singleton<TooltipHandler>
{
    [SerializeField] TMPro.TMP_Text textUI, subTextUI;

    Transform currentTarget;

    private void Update()
    {
        transform.position = Util.MouseToWorld(CameraController.Instance.Camera);
    }

    public void Display(Transform target, string text, string subText)
    {
        currentTarget = target;
        textUI.text = text;
        subTextUI.text = subText;
     }

    public void StopDisplaying(Transform transform)
    {
        if(currentTarget = transform)
        {
            currentTarget = null;
            textUI.text = "";
            subTextUI.text = "";
        }
    }

}
