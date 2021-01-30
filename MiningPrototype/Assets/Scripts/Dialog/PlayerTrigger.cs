using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : StateListenerBehaviour
{


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerController psm))
        {
            OnPlayerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IPlayerController psm))
        {
            OnPlayerExit();
        }
    }


    public virtual void OnPlayerEnter()
    {
    }

    public virtual void OnPlayerExit()
    {
    }

}
