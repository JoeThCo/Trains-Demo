using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public Node ToNode { get; private set; }
    public Node FromNode { get; private set; }

    public Edge(Node toNode, Node fromNode, float weight = 1f)
    {
        this.ToNode = toNode;
        this.FromNode = fromNode;
    }
}