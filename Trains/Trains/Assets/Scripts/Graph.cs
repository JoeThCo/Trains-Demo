using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        Nodes = MakeNodes(GetKnots(splineContainer).Values.ToArray());
        Debug.Log($"Nodes: {Nodes.Count}");
    }
    #region Nodes
    private Dictionary<Vector3, BezierKnot> GetKnots(SplineContainer splineContainer)
    {
        Dictionary<Vector3, BezierKnot> dict = new Dictionary<Vector3, BezierKnot>();

        foreach (Spline spline in splineContainer.Splines)
            foreach (BezierKnot knot in spline.Knots)
                if (!dict.ContainsKey(knot.Position))
                    dict[knot.Position] = knot;

        return dict;
    }

    private List<Node> MakeNodes(BezierKnot[] uniquePositions)
    {
        List<Node> output = new List<Node>();
        foreach (BezierKnot knot in uniquePositions)
            output.Add(new Node(output.Count, knot));

        return output;
    }
    #endregion
}