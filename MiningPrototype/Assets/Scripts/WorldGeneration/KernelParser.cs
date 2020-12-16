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

    private void LoadKernels()
    {
        List<Kernel> kernels = new List<Kernel>();
        foreach (var k in kernelFiles)
        {
            AddKernelsFrom(k, kernels);
        }

        this.kernels = kernels.ToArray();
    }

    private void AddKernelsFrom(TextAsset k, List<Kernel> kernels)
    {
        string[] lines = k.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        List<string> currentLines = new List<string>();
        int index = 0;

        while (index < lines.Length)
        {
            string currentLine = lines[index].Replace(" ",""); //Remove space character
            if (currentLine.StartsWith("//"))
            {
                //do nothing on comment
            }
            else if (string.IsNullOrWhiteSpace(currentLine))
            {
                if (currentLines.Count > 0)
                {
                    kernels.Add(Kernel.FromStrings(currentLines.ToArray()));
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
