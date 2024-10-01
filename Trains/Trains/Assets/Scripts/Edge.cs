using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Edge
{
    public int Index { get; private set; }
    public int InverseIndex { get; private set; }
    public Node FromNode { get; private set; }
    public Node ToNode { get; private set; }
    public float Weight { get; private set; }
    public Vector3 EdgeDireciton { get; private set; }
    public bool IsLessThanEdge { get; private set; }

    public Edge(int index, Node fromNode, Node toNode)
    {
        Index = index;

        FromNode = fromNode;
        ToNode = toNode;

        EdgeDireciton = (FromNode.Position - ToNode.Position).normalized;
        Weight = Vector3.Distance(FromNode.Position, ToNode.Position);

        IsLessThanEdge = fromNode.Index < toNode.Index;
        InverseIndex = IsLessThanEdge ? Index + 1 : Index - 1;
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

    public string GetKnotsToString()
    {
        string output = string.Empty;
        foreach (BezierKnot knot in GetKnots())
        {
            output += knot.Position.ToString();
        }
        return output;
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
        return FromNode.Equals(other.FromNode) && ToNode.Equals(other.ToNode);
    }

    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }
}