using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeVisualization : MonoBehaviour
{
    private Node node;

    public void NodeVisualizationInit(Node node)
    {
        this.node = node;
        gameObject.name = $"{node}";
    }
}