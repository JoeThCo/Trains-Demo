using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeVisualization : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    public void EdgeVisualizationInit(Edge edge)
    {
        gameObject.name = $"{edge}";

        //line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, edge.ToNode.Position);
        lineRenderer.SetPosition(1, edge.FromNode.Position);
    }
}