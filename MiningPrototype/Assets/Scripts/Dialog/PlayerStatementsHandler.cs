using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatementsHandler : MonoBehaviour
{
    [SerializeField] DialogElementVisualization textPrefab;
    [SerializeField] Canvas textCanvas;
    [SerializeField] RectTransform textSpawnPosition;

    public void Say(string msg, float duration)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            DialogElementVisualization text = Instantiate(textPrefab, textSpawnPosition.position, Quaternion.identity, textCanvas.transform); //Safe
            text.Init(null, msg, duration);
        }
    }
}
