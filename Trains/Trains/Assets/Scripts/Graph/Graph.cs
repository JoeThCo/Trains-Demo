using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class Graph
{
    public Node[] Nodes { get; private set; }
    public Edge[] Edges { get; private set; }
    public int[,] AdjacencyMatrix { get; private set; }
    public Dictionary<Vector3, Node> PositionToNodeMap { get; private set; }
    public Dictionary<Edge, List<Edge>> EdgeConnectionMap { get; private set; }
    public static System.Random Random { get; private set; }

    public Graph(SplineContainer splineContainer)
    {
        //todo move this somewhere else
        Random = new System.Random(0);

        //Nodes
        Nodes = CreateNodes(GetUniqueKnots(splineContainer).Values.ToArray()).ToArray();
        PositionToNodeMap = CreateNodePositionMap(Nodes);

        //edges
        Edges = CreateEdges(splineContainer).ToArray();

        //node connections
        AdjacencyMatrix = new int[Nodes.Length, Nodes.Length];
        AdjacencyMatrix = CreateAdjacencyMatrix(Nodes, Edges);

        //node degrees
        SetDegrees(Nodes, AdjacencyMatrix);
        Debug.Log($"Graph Info: Nodes: {Nodes.Length} | Edges: {Edges.Length}");

        //edge connections
        EdgeConnectionMap = RemoveInvalidConnections(CreateEdgeDictionary(Edges));
        foreach (KeyValuePair<Edge, List<Edge>> kvp in EdgeConnectionMap)
        {
            string output = string.Empty;
            foreach (Edge edge in kvp.Value)
            {
                output += $"{edge.ToString()}, ";
            }
            Debug.Log($"{kvp.Key.ToString()} | {output}");
        }
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

    private Dictionary<Vector3, Node> CreateNodePositionMap(Node[] nodes)
    {
        Dictionary<Vector3, Node> output = new Dictionary<Vector3, Node>();
        foreach (Node node in nodes)
            output[node.Position] = node;
        return output;
    }

    private void SetDegrees(Node[] nodes, int[,] adjacencyMatrix)
    {
        foreach (Node node in nodes)
            node.SetDegrees(GetDegrees(node, adjacencyMatrix));
    }

    private Degrees GetDegrees(Node node, int[,] adjacencyMatrix)
    {
        int inDegree = 0;
        int outDegree = 0;
        for (int i = 0; i < Nodes.Length; i++)
        {
            inDegree += adjacencyMatrix[i, node.Index];
            outDegree += adjacencyMatrix[node.Index, i];
        }
        return new Degrees(inDegree, outDegree);
    }
    #endregion

    #region Edge
    private List<Edge> CreateEdges(SplineContainer splineContainer)
    {
        List<Edge> output = new List<Edge>();
        int globalIndex = 0;

        foreach (Spline spline in splineContainer.Splines)
        {
            BezierKnot[] knots = spline.Knots.ToArray();
            for (int i = 0; i < knots.Length - 1; i++)
            {
                Node lessThanNode = PositionToNodeMap[knots[i].Position];
                Node greaterThanNode = PositionToNodeMap[knots[i + 1].Position];
                if (lessThanNode.Equals(greaterThanNode)) continue;

                output.Add(new Edge(globalIndex++, lessThanNode, greaterThanNode));
                output.Add(new Edge(globalIndex++, greaterThanNode, lessThanNode));
            }

            if (spline.Closed)
            {
                Node lessThanNode = PositionToNodeMap[knots[knots.Length - 1].Position];
                Node greaterThanNode = PositionToNodeMap[knots[0].Position];
                if (lessThanNode.Equals(greaterThanNode)) continue;

                output.Add(new Edge(globalIndex++, lessThanNode, greaterThanNode));
                output.Add(new Edge(globalIndex++, greaterThanNode, lessThanNode));
            }
        }
        return output;
    }

    private int[,] CreateAdjacencyMatrix(Node[] nodes, Edge[] edges)
    {
        int[,] output = new int[nodes.Length, nodes.Length];
        foreach (Edge edge in edges)
            output[edge.FromNode.Index, edge.ToNode.Index] = 1;
        return output;
    }

    private Dictionary<Edge, List<Edge>> CreateEdgeDictionary(Edge[] allEdges)
    {
        Dictionary<Node, List<Edge>> edgesStartingFromNode = new Dictionary<Node, List<Edge>>();
        foreach (Edge edge in allEdges)
        {
            if (!edgesStartingFromNode.ContainsKey(edge.FromNode))
                edgesStartingFromNode[edge.FromNode] = new List<Edge>();
            edgesStartingFromNode[edge.FromNode].Add(edge);
        }

        Dictionary<Edge, List<Edge>> dict = new Dictionary<Edge, List<Edge>>();
        foreach (Edge firstEdge in allEdges)
        {
            if (edgesStartingFromNode.TryGetValue(firstEdge.ToNode, out List<Edge> connectedEdges))
            {
                foreach (Edge secondEdge in connectedEdges)
                {
                    if (firstEdge.Equals(secondEdge)) continue;
                    if (firstEdge.FromNode.Equals(secondEdge.ToNode)) continue;

                    if (!dict.ContainsKey(firstEdge))
                        dict[firstEdge] = new List<Edge>();
                    dict[firstEdge].Add(secondEdge);
                }
            }
        }
        return dict;
    }

    private Dictionary<Edge, List<Edge>> RemoveInvalidConnections(Dictionary<Edge, List<Edge>> input)
    {
        Dictionary<Edge, List<Edge>> tempDict = new Dictionary<Edge, List<Edge>>();
        foreach (KeyValuePair<Edge, List<Edge>> kvp in input)
        {
            Edge keyEdge = kvp.Key;
            if (!keyEdge.ToNode.IsJunction)
                tempDict[keyEdge] = kvp.Value;
            else
            {
                List<Edge> validEdges = new List<Edge>();
                foreach (Edge edge in kvp.Value)
                {
                    float dot = keyEdge.GetDot(edge);
                    if (dot < 0) continue;

                    float angle = keyEdge.GetAngleDifference(edge);
                    if (angle >= 60) continue;

                    validEdges.Add(edge);
                }
                tempDict[keyEdge] = validEdges;
            }
        }
        return tempDict;
    }
    #endregion
}