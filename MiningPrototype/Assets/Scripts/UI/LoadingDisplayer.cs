using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingDisplayer : StateListenerBehaviour
{
    [SerializeField] Image image;
    [SerializeField] float rotSpeed;

    private void Update()
    {
        image.transform.Rotate(0, 0, Time.unscaledDeltaTime * rotSpeed);
    }

    protected override void OnStartAfterLoad()
    {
        Destroy(gameObject);
    }
}
