using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ILayeredUI
{
    void ForceClose();
}

public class UIsHandler : MonoBehaviour
{
    List<ILayeredUI> layeredUIs = new List<ILayeredUI>();

    [Zenject.Inject] PauseMenu pauseMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (layeredUIs.Count > 0)
            {
                int index = layeredUIs.Count - 1;
                var ui = layeredUIs[index];
                layeredUIs.RemoveAt(index);
                ui.ForceClose();
            }
            else
            {
                pauseMenu.TogglePause();
            }
        }
    }


    public void NotifyOpening(ILayeredUI ui)
    {
        if (!layeredUIs.Contains(ui))
        {
            layeredUIs.Add(ui);
        }
    }

    public void NotifyClosing(ILayeredUI ui)
    {
        if (layeredUIs.Contains(ui))
        {
            layeredUIs.Remove(ui);
        }
    }
    

}
