using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiroImporterWindow : EditorWindow
{


    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/MiroImporter")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MiroImporterWindow window = (MiroImporterWindow)EditorWindow.GetWindow(typeof(MiroImporterWindow));
        window.Show();
    }



    void OnGUI()
    {

        if (GUILayout.Button("Update StringTrees from Miro Json file"))
        {
            string path = EditorUtility.OpenFilePanel("Select Miro JSON file", "", "json");
            MiroParser.UpdateStringTreesFromMiroJsonFile(path);
        }

        if(GUILayout.Button("Test Convert to AltarTree"))
        {
            MiroParser.LoadTreesAsAltarTreeCollection();
        }

    }
}
