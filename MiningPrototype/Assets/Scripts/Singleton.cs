using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get => instance;
    }

    protected virtual void Awake()
    {
        if(Instance == null)
        {
            instance = (T)this;
        }
        else
        {
            Destroy(this);
            Debug.LogError("Found multiple " + typeof(T).ToString());
        }
    }

}
