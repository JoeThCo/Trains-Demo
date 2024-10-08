using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EdgeVisualization : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    public void EdgeVisualizationInit(Edge edge)
    {
        gameObject.name = $"#{edge.Index} {edge}";

        //line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, edge.FromNode.Position);
        lineRenderer.SetPosition(1, edge.ToNode.Position);
    }
}