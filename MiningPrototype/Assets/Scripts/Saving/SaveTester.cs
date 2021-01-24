using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTester : MonoBehaviour
{
#if UNITY_EDITOR

    [Zenject.Inject] SaveHandler saveHandler;
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.F4))
            Save();
        else if (Input.GetKeyDown(KeyCode.F5))
            Load();



    }

    [Button(null, EButtonEnableMode.Playmode)]
    public void Save()
    {
        saveHandler.Save();
    }

    [Button(null, EButtonEnableMode.Playmode)]
    public void Load()
    {
        SaveHandler.LoadFromSavefile = true;
        //Reload self
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

#endif
}
