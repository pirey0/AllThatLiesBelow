using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiroTester : MonoBehaviour
{
    
    [NaughtyAttributes.Button]
    public void TestRun()
    {
        MiroParser.TestRun();
    }

}
