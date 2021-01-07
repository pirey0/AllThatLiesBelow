using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : MineableObject
{
    [SerializeField] ItemDrops drops;

    [Zenject.Inject] CameraController cameraController;

    protected override void Destroyed()
    {
        contains = drops.GetRandomDrop();
        cameraController.Shake(transform.position,CameraShakeType.explosion,0.25f,10,0.25f);
        base.Destroyed();
    }
}
