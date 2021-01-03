using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogEntry : MonoBehaviour
{

    [SerializeField] VariablePair[] variables;


    public bool IsAccessible(ProgressionHandler progressionHandler)
    {
        foreach (var v in variables)
        {
            if(v.State != progressionHandler.GetVariable(v.Name))
            {
                return false;
            }
        }
        return true;
    }

}


[System.Serializable]
public struct VariablePair
{
    public string Name;
    public bool State;
}