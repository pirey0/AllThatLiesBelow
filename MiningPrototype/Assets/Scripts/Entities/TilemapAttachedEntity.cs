using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapAttachedEntity : MonoBehaviour, ITileUpdateReceiver
{
    [SerializeField] protected Vector2Int tileOffsetToAttachTo;

    [Zenject.Inject] RuntimeProceduralMap map;

    void Start()
    {
        var pos = GetAttachCoordinates();
        map.AddToReceiverMapAt(pos.x, pos.y, this);
    }

    private void OnDestroy()
    {
        if(map != null)
        {
            var pos = GetAttachCoordinates();
            map.RemoveFromReceiverMapAt(pos.x, pos.y, this);
        }
    }

    private Vector2Int GetAttachCoordinates()
    {
        return transform.position.ToGridPosition() + tileOffsetToAttachTo;
    }

    public void OnTileChanged(int x, int y, TileUpdateReason reason)
    {
        if(reason == TileUpdateReason.Collapse || reason == TileUpdateReason.Destroy)
        {
            Destroy(gameObject);
        }
    }

    public void OnTileCrumbleNotified(int x, int y)
    {
    }

    public void OnTileUpdated(int x, int y)
    {

    }

    private void OnDrawGizmosSelected()
    {
        Util.GizmosDrawTile(GetAttachCoordinates());
    }


}
