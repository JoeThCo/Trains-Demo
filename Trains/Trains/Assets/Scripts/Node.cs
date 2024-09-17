using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Node
{
    public int Index { get; private set; }
    public BezierKnot Knot { get; private set; }
    public Vector3 Position { get; private set; }
    public Node(int index, BezierKnot knot)
    {
        Index = index;
        Knot = knot;
        Position = knot.Position;
    }
}