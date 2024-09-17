using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{

    private SplineContainer splineContainer;
    public void SplitGraph()
    {
        splineContainer = FindObjectOfType<SplineContainer>();
    }
}