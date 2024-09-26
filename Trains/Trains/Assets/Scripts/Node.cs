using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Node
{
    public int Index { get; private set; }
    public BezierKnot Knot { get; private set; }
    public Vector3 Position { get; private set; }
    public Degrees Degrees { get; private set; }

    public bool IsJunction { get; private set; }
    public bool IsEnd {  get; private set; }

    public Node(int index, BezierKnot knot)
    {
        Index = index;
        Knot = knot;
        Position = knot.Position;
    }

    public void SetDegrees(Degrees degrees)
    {
        Degrees = degrees;
        IsJunction = Degrees.InDegree > 2 || Degrees.OutDegree > 2;
        IsEnd = Degrees.InDegree <= 1 && Degrees.OutDegree <= 1;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Node)) return false;
        Node other = (Node)obj;
        return Index == other.Index;
    }

    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Index}";
    }
}