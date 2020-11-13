using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase
{
    private void Start()
    {
        AdaptHeightTo(CalculateHeight());
    }

}
