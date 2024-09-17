using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Graph
{
    public List<Node> Nodes { get; private set; }
    public List<Edge> Edges { get; private set; }

    public Graph(SplineContainer splineContainer)
    {
        Nodes = new List<Node>();
        Edges = new List<Edge>();
    }
}