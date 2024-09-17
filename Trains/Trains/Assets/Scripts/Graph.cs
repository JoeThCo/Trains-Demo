using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Graph
{
    public List<Node> Nodes { get; private set; }
    public List<Edge> Edges { get; private set; }

    private Dictionary<Vector3, Node> PositionToNodeMap;

    public Graph(SplineContainer splineContainer)
    {
        Nodes = new List<Node>();
        Edges = new List<Edge>();

        Nodes = CreateNodes(GetUniqueKnots(splineContainer).Values.ToArray());
        Debug.Log($"Nodes: {Nodes.Count}");

        PositionToNodeMap = CreateNodePositionMap(Nodes);
        Edges = CreateEdges(splineContainer);
        Debug.Log($"Edges: {Edges.Count}");
    }

    #region Nodes
    private Dictionary<Vector3, BezierKnot> GetUniqueKnots(SplineContainer splineContainer)
    {
        Dictionary<Vector3, BezierKnot> dict = new Dictionary<Vector3, BezierKnot>();

        foreach (Spline spline in splineContainer.Splines)
            foreach (BezierKnot knot in spline.Knots)
                if (!dict.ContainsKey(knot.Position))
                    dict[knot.Position] = knot;

        return dict;
    }

    private List<Node> CreateNodes(BezierKnot[] uniquePositions)
    {
        List<Node> output = new List<Node>();
        foreach (BezierKnot knot in uniquePositions)
            output.Add(new Node(output.Count, knot));

        return output;
    }

    private Dictionary<Vector3, Node> CreateNodePositionMap(List<Node> nodes)
    {
        Dictionary<Vector3, Node> output = new Dictionary<Vector3, Node>();
        foreach (Node node in nodes)
            output[node.Position] = node;

        return output;
    }
    #endregion

    #region Edge
    private List<Edge> CreateEdges(SplineContainer splineContainer)
    {
        List<Edge> output = new List<Edge>();

        foreach (Spline spline in splineContainer.Splines)
        {
            BezierKnot[] knots = spline.Knots.ToArray();

            for (int i = 0; i < knots.Length - 1; i++)
            {
                Vector3 posA = knots[i].Position;
                Vector3 posB = knots[i + 1].Position;

                Node nodeA = PositionToNodeMap[posA];
                Node nodeB = PositionToNodeMap[posB];

                output.Add(new Edge(nodeA, nodeB));
                output.Add(new Edge(nodeB, nodeA));
            }

            if (spline.Closed)
            {
                Vector3 posA = knots[knots.Length - 1].Position;
                Vector3 posB = knots[0].Position;

                Node nodeA = PositionToNodeMap[posA];
                Node nodeB = PositionToNodeMap[posB];

                output.Add(new Edge(nodeA, nodeB));
                output.Add(new Edge(nodeB, nodeA));
            }
        }

        return output;
    }
    #endregion
}