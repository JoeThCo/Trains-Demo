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
    }

    public override string ToString()
    {
        return $"{Index}";
    }
}