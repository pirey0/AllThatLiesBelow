using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KernelParser : MonoBehaviour
{
    [SerializeField] TextAsset[] kernelFiles;

    Kernel[] kernels;

    private void Awake()
    {
        LoadKernels();
    }

    public Kernel[] GetAllKernels()
    {
        return kernels;
    }

    [NaughtyAttributes.Button]
    private void LoadKernels()
    {
        List<Kernel> kernels = new List<Kernel>();
        foreach (var k in kernelFiles)
        {
            if (k != null)
                AddKernelsFrom(k, kernels);
        }

        this.kernels = kernels.ToArray();
        Debug.Log("Loaded " + this.kernels.Length + " Kernels");
    }

    private void AddKernelsFrom(TextAsset k, List<Kernel> kernels)
    {
        string[] lines = k.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        List<string> currentLines = new List<string>();
        int index = 0;

        while (index < lines.Length)
        {
            string currentLine = lines[index].Replace(" ", ""); //Remove space character
            if (currentLine.StartsWith("//"))
            {
                //do nothing on comment
            }
            else if (string.IsNullOrWhiteSpace(currentLine))
            {
                if (currentLines.Count > 0)
                {
                    Kernel ker = Kernel.FromStrings(currentLines.ToArray());
                    if (ker == null)
                    {
                        Debug.LogError("Null Kernel on line " + index + ": " + currentLine);
                    }
                    else
                    {
                        kernels.Add(ker);
                    }
                    currentLines.Clear();
                }
            }
            else
            {
                currentLines.Add(currentLine);
            }
            index++;
        }
    }
}
