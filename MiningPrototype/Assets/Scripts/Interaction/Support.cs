using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase
{
    private void Start()
    {
        AdaptHeightTo(CalculateHeight());
        Carve();
    }

    public override void OnTileCrumbleNotified(int x, int y)
    {
        UnCarvePrevious();
        Debug.Log("Support crumbled.");
        Destroy(gameObject);
        //Support broke... What happens now?
    }
}
