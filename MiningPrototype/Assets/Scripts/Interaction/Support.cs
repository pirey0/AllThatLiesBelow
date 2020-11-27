using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Support : SupportBase, INonPersistantSavable
{
    private void Start()
    {
        AdaptHeightTo(CalculateHeight());
        Carve();
    }


    public override void OnTileUpdated(int x, int y, TileUpdateReason reason)
    {
        if(this != null && reason == TileUpdateReason.Destroy)
        {
            Debug.Log("Support destroyed.");
            UncarveDestroy();
        }
    }

    public override void OnTileCrumbleNotified(int x, int y)
    {
        UncarveDestroy();
        //Support broke... What happens now?
    }

    public SpawnableSaveData ToSaveData()
    {
        var data = new SpawnableSaveData();
        data.SpawnableIDType = SpawnableIDType.Support;
        data.Position = new SerializedVector3(transform.position);
        data.Rotation = new SerializedVector3(transform.eulerAngles);

        return data;
    }

    public void Load(SpawnableSaveData data)
    {
    }
}
