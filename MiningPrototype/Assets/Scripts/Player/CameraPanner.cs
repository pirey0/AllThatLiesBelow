using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanner : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] float overWorldOffset;
    [SerializeField] float transitionSpeed;

    [Zenject.Inject] PlayerStateMachine player;
    [Zenject.Inject] PlayerInteractionHandler interactionHandler;

    float yOffset;

    private void Update()
    {
        UpdatePosition();
    }

    private void OnPreRender()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (Time.timeScale == 0)
            return;

        Vector3 dir = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        dir = dir - new Vector3(0.5f, 0.5f);
        dir = new Vector3(dir.x.Sign() * curve.Evaluate(dir.x.Abs()), dir.y.Sign() * curve.Evaluate(dir.y.Abs()));


        if (player.InOverworld() || interactionHandler.InventoryDisplayState == InventoryState.Open)
            yOffset += transitionSpeed * Time.deltaTime;
        else
            yOffset -= transitionSpeed * Time.deltaTime;

        yOffset = Mathf.Clamp(yOffset, 0, overWorldOffset);

        transform.position = player.transform.position + dir + new Vector3(0, yOffset);
    }
}
