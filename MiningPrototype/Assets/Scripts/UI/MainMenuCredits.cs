using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MainMenuCredits : ScriptableObject
{
    [TextArea(15,100)] public string BigText;
    public int BigTextSize;

    [TextArea(15,100)] public string SmallText;
    public int SmallTextSize;

    [TextArea(15,100)] public string SpecialText;
    public int SpecialTextSize;
}
