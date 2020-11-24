using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InWorldCanvas : MonoBehaviour
{
    [Zenject.Inject] CameraController cameraController;

    private Canvas canvas;
    public Canvas Canvas {get=> canvas; }
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        canvas.worldCamera = cameraController.Camera;
    }
}
