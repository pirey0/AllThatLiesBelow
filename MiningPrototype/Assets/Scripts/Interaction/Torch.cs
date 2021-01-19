using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Torch : TilemapCarvingEntity
{
    [SerializeField] float z = 0.9f;
    [SerializeField] AnimationCurve innerRadiusOverY, outerRadiusOverY;

    private void Start ()
    {
        //torches are in the background (z)
        transform.position = new Vector3(transform.position.x, transform.position.y, z);

        var pl = GetComponent<Light2D>();

        float y = transform.position.y;
        float ir = innerRadiusOverY.Evaluate(y);
        float or = outerRadiusOverY.Evaluate(y);
        pl.pointLightInnerRadius = ir;
        pl.pointLightOuterRadius = or;
        Carve();
    }

    public override void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if(reason == TileUpdateReason.Destroy)
        {
            UncarveDestroy();
        }
    }
}
