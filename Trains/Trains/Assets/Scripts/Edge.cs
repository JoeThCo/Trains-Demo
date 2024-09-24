using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Edge
{
    public int Index { get; private set; }
    public Node FromNode { get; private set; }
    public Node ToNode { get; private set; }
    public float Weight { get; private set; }
    public Vector3 EdgeDireciton { get; private set; }

    public Edge(int index, Node fromNode, Node toNode)
    {
        Index = index;

        FromNode = fromNode;
        ToNode = toNode;

        EdgeDireciton = (FromNode.Position - ToNode.Position).normalized;
        Weight = Vector3.Distance(FromNode.Position, ToNode.Position);
    }

    public float GetDot(Edge otherEdge)
    {
        return Vector3.Dot(EdgeDireciton, otherEdge.EdgeDireciton);
    }

    public float GetAngleDifference(Edge otherEdge)
    {
        return Vector3.Angle(EdgeDireciton, otherEdge.EdgeDireciton);
    }

    public List<BezierKnot> GetKnots()
    {
        return new List<BezierKnot> { FromNode.Knot, ToNode.Knot };
    }

    public Vector3 GetHalfWay()
    {
        return Vector3.Lerp(FromNode.Position, ToNode.Position, .5f);
    }

    public override string ToString()
    {
        return $"F:{FromNode.Index} -> T:{ToNode.Index}";
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Edge)) return false;
        Edge other = obj as Edge;
        return Index == other.Index;
    }

    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }
}