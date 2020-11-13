using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMinableNonGrid
{
    void MouseLeave();
    void MouseEnter();
    Vector2 GetPosition();
    void Damage(float v);
}
