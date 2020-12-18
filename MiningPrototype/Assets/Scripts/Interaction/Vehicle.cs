using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IVehicle
{
    bool ConsumesVerticalInput();
    bool ConsumesHorizontalInput();

    void EnteredBy(PlayerStateMachine player);
    void LeftBy(PlayerStateMachine player);
}

public class Vehicle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
