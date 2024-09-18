using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Edge
{
    public int Index { get; private set; }
    public Node ToNode { get; private set; }
    public Node FromNode { get; private set; }
    public float Weight { get; private set; }

    public Edge(int index, Node toNode, Node fromNode)
    {
        Index = index;

        ToNode = toNode;
        FromNode = fromNode;

        Weight = Vector3.Distance(ToNode.Position, fromNode.Position);
    }

    public List<BezierKnot> GetKnots()
    {
        return new List<BezierKnot> { ToNode.Knot, FromNode.Knot };
    }

    public Vector3 GetHalfWay()
    {
        return Vector3.Lerp(ToNode.Position, FromNode.Position, .5f);
    }

    public override string ToString()
    {
        return $"{ToNode.Index} -> {FromNode.Index}";
    }
}