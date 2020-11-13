using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selfdestruct : MonoBehaviour
{
    [SerializeField] float timeTillDestroy;
    void Start()
    {
        Invoke("Selfdestroy", timeTillDestroy);
    }

    private void Selfdestroy()
    {
        Destroy(gameObject);
    }
}
