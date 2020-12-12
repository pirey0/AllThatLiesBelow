using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TooltipHandler : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text textUI, subTextUI;
    [SerializeField] Image box;

    [Zenject.Inject] CameraController cameraController;

    Transform currentTarget;


    void LateUpdate() 
    {
        transform.position = Util.MouseToWorld(cameraController.Camera);
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
        if (currentTarget = transform)
        {
            currentTarget = null;
            textUI.text = "";
            subTextUI.text = "";
            box.enabled = false;
        }
    }

}
