using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{
    private SplineContainer splineContainer;

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

        splineContainer = FindObjectOfType<SplineContainer>();
        NodePrefab = Resources.Load<GameObject>("Node");

        Graph graph = new Graph(splineContainer);

        foreach (Node node in graph.Nodes)
        {
            GameObject nodeGameObject = Instantiate(NodePrefab, node.Position, Quaternion.identity, transform);
            nodeGameObject.name = node.Index.ToString();
        }
    }
}