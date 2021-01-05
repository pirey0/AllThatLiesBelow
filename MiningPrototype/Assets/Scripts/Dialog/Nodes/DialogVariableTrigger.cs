using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogVariableTrigger : MonoBehaviour
{
    [SerializeField] string variableName;
    [SerializeField] bool setToTrue = true;
    [Zenject.Inject] ProgressionHandler progressionHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerStateMachine psm))
        {
            progressionHandler.SetVariable(variableName, setToTrue);
            Debug.Log("Trigger: Set" + variableName + " " + setToTrue);
        }
    }

    private void OnValidate()
    {
        name = "DialogVariableTrigger (" + (string.IsNullOrEmpty(variableName) ? "Empty" : variableName + " " + setToTrue) + ")";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
       UnityEditor.Handles.Label(transform.position,  (string.IsNullOrEmpty(variableName) ? "Empty" : variableName + " " + setToTrue));
    }
#endif
}
