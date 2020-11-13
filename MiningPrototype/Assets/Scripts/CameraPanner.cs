using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanner : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] AnimationCurve curve;
    private void Update()
    {
        UpdatePosition();
    }

    private void OnPreRender()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 dir = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        dir = dir - new Vector3(0.5f, 0.5f);
        dir = new Vector3(dir.x.Sign() * curve.Evaluate(dir.x.Abs()), dir.y.Sign() * curve.Evaluate(dir.y.Abs()));

        transform.position = player.position + dir;
    }

}
