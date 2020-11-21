using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DurationTracker 
{
    System.Diagnostics.Stopwatch stopwatch;
    string name;
    public DurationTracker(string name)
    {
        this.name = name;
        stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
    }


    public void Stop()
    {
        stopwatch.Stop();
        Debug.Log(name + " took " + stopwatch.ElapsedMilliseconds + "ms");
    }
}
