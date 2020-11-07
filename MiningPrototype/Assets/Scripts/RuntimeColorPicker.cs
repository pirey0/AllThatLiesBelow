using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeColorPicker : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] int startX, startY, width, colorSpacing, blockSpacing;
    
    Color[] colors = new Color[8];
    bool on;
    bool updateContinuosly;
    private void Start()
    {
        if(material == null)
        {
            Destroy(this);
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                colors[i] = material.GetColor("_Color" + (i+1));
            }
        }
    }

    private void UpdateColors()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            material.SetColor("_Color" + (i+1), colors[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            on = !on;

        if (updateContinuosly)
        {
            UpdateColors();
        }
    }

    private void OnGUI()
    {
        if (!on)
            return;

        for (int i = 0; i < 8; i++)
        {
            colors[i] = CreateGUIColor(colors[i], new Rect(startX, startY + blockSpacing * i, width, 20), i.ToString());
            
        }   

        if(GUI.Button(new Rect(startX, startY-40, 100, 20), "Update"))
        {
            UpdateColors();
        }
        if (GUI.Button(new Rect(startX + 120, startY - 40, 100, 20), updateContinuosly? "Disable AutoUpdate" : "Enable AutoUpdate"))
        {
            updateContinuosly = !updateContinuosly;
        }
    }


    private Color CreateGUIColor(Color source, Rect startRect, string title)
    {
        GUI.color = source;
        GUI.Label(startRect, title);
        startRect.x += 50;
        GUI.Label(startRect, "R");
        startRect.x += 20;
        source.r = GUI.HorizontalSlider(startRect, source.r, 0, 1);
        startRect.x += width;
        GUI.Label(startRect, "G");
        startRect.x += 20;
        source.g = GUI.HorizontalSlider(startRect , source.g, 0, 1);
        startRect.x += width;
        GUI.Label(startRect, "B");
        startRect.x += 20;
        source.b = GUI.HorizontalSlider(startRect, source.b, 0, 1);
        startRect.x += width;
        GUI.Label(startRect, "A");
        startRect.x += 20;
        source.a = GUI.HorizontalSlider(startRect, source.a, 0, 1);

        return source;
    }

}
