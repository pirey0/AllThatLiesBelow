using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KernelIterator : StateListenerBehaviour
{
    [SerializeField] int sizeX, sizeY;

    [Zenject.Inject] KernelParser kernelParser;
    [Zenject.Inject] RuntimeProceduralMap map;
    [Zenject.Inject] PlayerStateMachine player;

    Kernel[] kernels;

    int currentStartX;
    int currentStartY;

    protected override void OnRealStart()
    {
        kernels = kernelParser.GetAllKernels();
        StartCoroutine(KernelRoutine());
    }

    private IEnumerator KernelRoutine()
    {
        int currentIndex = 0;
        currentStartX = player.transform.position.ToGridPosition().x - sizeX / 2;
        currentStartY = player.transform.position.ToGridPosition().y - sizeY / 2;
        int x = currentStartX;
        int y = currentStartY;

        while (true)
        {
            Kernel currentKernel = kernels[currentIndex];

            if (IsMatch(currentKernel, x, y))
            {
                Debug.Log("Match at: (" + x + "/" + y + ")");
            }

            x++;
            if (x >= map.SizeX - currentKernel.Width)
            {
                x = currentStartX;
                y++;

                if (y >= map.SizeY - currentKernel.Height)
                {
                    y = currentStartY;
                    yield return null; //1 kernel per frame
                    currentIndex++;
                    
                    if(currentIndex >= kernels.Length) //finished all kernels
                    {
                        currentIndex = 0;
                        currentStartX = player.transform.position.ToGridPosition().x - sizeX / 2;
                        currentStartY = player.transform.position.ToGridPosition().y - sizeY / 2;
                        x = currentStartX;
                        y = currentStartY;
                    }
                }
            }
        }
    }

    public bool IsMatch(Kernel k, int px, int py)
    {
        for (int y = 0; y < k.Height; y++)
        {
            for (int x = 0; x < k.Width; x++)
            {
                if (k[x, y] != map.GetTileInfo(map[px + x, py + y].Type).CrumbleType)
                {
                    return false;
                }
            }
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(player!= null)
        {
            Vector3 pos = player.transform.position.ToGridPosition().AsV3();
            Gizmos.DrawWireCube(pos, new Vector3(sizeX, sizeY, 0));
            UnityEditor.Handles.Label(pos + new Vector3(-sizeX / 2, sizeY / 2, 0), "KernelIterator");
        }
    }
#endif
}
