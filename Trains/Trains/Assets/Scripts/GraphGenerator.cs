using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{
    private SplineContainer inputSplineContainer;

    private NodeVisualization NodePrefab;
    private EdgeVisualization EdgePrefab;

    public static Graph Graph;

    public void DeleteDebug()
    {
        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    public void SplitSplines()
    {
        DeleteDebug();

        inputSplineContainer = FindObjectOfType<SplineContainer>();
        NodePrefab = Resources.Load<NodeVisualization>("Node");
        EdgePrefab = Resources.Load<EdgeVisualization>("Edge");

        Graph = new Graph(inputSplineContainer);
        foreach (Node node in Graph.Nodes)
        {
            NodeVisualization nodeVisulization = Instantiate(NodePrefab, node.Position, Quaternion.identity, transform).GetComponent<NodeVisualization>();
            nodeVisulization.NodeVisualizationInit(node);
        }

        foreach (Edge edge in Graph.Edges)
        {
            EdgeVisualization edgeVisualization = Instantiate(EdgePrefab, edge.GetHalfWay(), Quaternion.identity, transform).GetComponent<EdgeVisualization>();
            edgeVisualization.EdgeVisualizationInit(edge);
        }

        GameObject outputSplineGameobject = new GameObject("Final Splines");
        outputSplineGameobject.transform.parent = transform;
        SplineContainer outputSplineContainer = outputSplineGameobject.AddComponent<SplineContainer>();
        outputSplineContainer.RemoveSplineAt(0);

        foreach (Edge edge in Graph.Edges)
        {
            Spline spline = new Spline(edge.GetKnots(), false);
            outputSplineContainer.AddSpline(spline);
        }

        foreach (Spline spline in outputSplineContainer.Splines)
        {
            spline.SetTangentMode(TangentMode.AutoSmooth);
        }
    }
}