using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Graph
{
    public List<Node> Nodes { get; private set; }
    public List<Edge> Edges { get; private set; }
    public int[,] AdjacencyMatrix { get; private set; }
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

        AdjacencyMatrix = new int[Nodes.Count, Nodes.Count];
        AdjacencyMatrix = CreateAdjacencyMatrix(Nodes, Edges);
        SetDegrees(Nodes);
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

    void SetDegrees(List<Node> nodes)
    {
        foreach (Node node in nodes)
        {
            (int, int) degrees = GetDegrees(node.Index);
            node.SetDegrees(degrees.Item1, degrees.Item2);
        }
    }

    public (int, int) GetDegrees(int index)
    {
        if (index < 0 || index >= Nodes.Count)
            return (0, 0);

        int inDegree = 0;
        int outDegree = 0;
        for (int i = 0; i < Nodes.Count; i++)
        {
            inDegree += AdjacencyMatrix[i, index];
            outDegree += AdjacencyMatrix[index, i];
        }

        return (inDegree, outDegree);
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

                output.Add(new Edge(output.Count, nodeA, nodeB));
                output.Add(new Edge(output.Count, nodeB, nodeA));
            }

            if (spline.Closed)
            {
                Vector3 posA = knots[knots.Length - 1].Position;
                Vector3 posB = knots[0].Position;

                Node nodeA = PositionToNodeMap[posA];
                Node nodeB = PositionToNodeMap[posB];

                output.Add(new Edge(output.Count, nodeA, nodeB));
                output.Add(new Edge(output.Count, nodeB, nodeA));
            }
        }

        return output;
    }

    private int[,] CreateAdjacencyMatrix(List<Node> nodes, List<Edge> edges)
    {
        int[,] output = new int[nodes.Count, nodes.Count];

        for (int y = 0; y < nodes.Count; y++)
            for (int x = 0; x < nodes.Count; x++)
                output[x, y] = 0;

        foreach (Edge edge in edges)
            AdjacencyMatrix[edge.FromNode.Index, edge.ToNode.Index] = 1;

        return output;
    }
    #endregion
}