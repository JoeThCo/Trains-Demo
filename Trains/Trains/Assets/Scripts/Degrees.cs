using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Degrees
{
    public int InDegree { get; private set; }
    public int OutDegree { get; private set; }

    public Degrees(int inDegree, int outDegree)
    {
        InDegree = inDegree;
        OutDegree = outDegree;
    }
}