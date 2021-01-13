using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MainMenuCredits : ScriptableObject
{
    [TextArea] public string BigText;
    public int BigTextSize;

    [TextArea] public string SmallText;
    public int SmallTextSize;
}
