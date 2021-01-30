using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IVehicle
{
    bool ConsumesVerticalInput();
    bool ConsumesHorizontalInput();

    void EnteredBy(IPlayerController player);
    void LeftBy(IPlayerController player);
}
