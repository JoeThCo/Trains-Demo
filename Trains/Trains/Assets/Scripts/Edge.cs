using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Edge
{
    public Node ToNode { get; private set; }
    public Node FromNode { get; private set; }

    public Edge(Node toNode, Node fromNode, float weight = 1f)
    {
        this.ToNode = toNode;
        this.FromNode = fromNode;
    }

    public List<BezierKnot> GetKnots()
    {
        return new List<BezierKnot> { ToNode.Knot, FromNode.Knot };
    }
}