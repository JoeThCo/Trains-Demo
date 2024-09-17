using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{
    private SplineContainer inputSplineContainer;

    private GameObject NodePrefab;

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
        NodePrefab = Resources.Load<GameObject>("Node");

        Graph graph = new Graph(inputSplineContainer);

        foreach (Node node in graph.Nodes)
        {
            GameObject nodeGameObject = Instantiate(NodePrefab, node.Position, Quaternion.identity, transform);
            nodeGameObject.name = $"Node {node.Index}";
        }

        foreach (Edge edge in graph.Edges)
        {
            //todo
        }

        GameObject outputSplineGameobject = new GameObject("Final Splines");
        outputSplineGameobject.transform.parent = transform;
        SplineContainer outputSplineContainer = outputSplineGameobject.AddComponent<SplineContainer>();

        outputSplineContainer.RemoveSplineAt(0);

        foreach (Edge edge in graph.Edges)
        {
            Spline spline = new Spline(edge.GetKnots(), false);
            outputSplineContainer.AddSpline(spline);
        }

        foreach (Spline spline in outputSplineContainer.Splines)
            spline.SetTangentMode(TangentMode.AutoSmooth);
    }
}