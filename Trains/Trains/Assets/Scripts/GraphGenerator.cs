using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] private bool DisplayDebug = false;
    [SerializeField][Range(.1f, 5f)] private float distanceStep = .5f;

    public static Graph Graph;

    private SplineContainer lessIndexToGreaterIndex;
    private SplineContainer greaterIndexToLessIndex;

    private SplineContainer finalSplineContainer;

    private NodeVisualization NodePrefab;
    private EdgeVisualization EdgePrefab;

    private GameObject nodesParent;
    private GameObject edgesParent;

    public void MakeFinalGraph()
    {
        DeleteDebug();

        SplineContainer inputSplineContainer = FindObjectOfType<SplineContainer>();
        NodePrefab = Resources.Load<NodeVisualization>("Node");
        EdgePrefab = Resources.Load<EdgeVisualization>("Edge");

        nodesParent = new GameObject("Nodes Parent");
        nodesParent.transform.parent = transform;

        edgesParent = new GameObject("Egdes Parent");
        edgesParent.transform.parent = transform;

        GameObject FromToToGameObject = new GameObject("From -> To Splines");
        FromToToGameObject.transform.parent = transform;
        lessIndexToGreaterIndex = FromToToGameObject.AddComponent<SplineContainer>();
        lessIndexToGreaterIndex.RemoveSplineAt(0);

        GameObject ToToFromGameObject = new GameObject("To -> From Splines");
        ToToFromGameObject.transform.parent = transform;
        greaterIndexToLessIndex = ToToFromGameObject.AddComponent<SplineContainer>();
        greaterIndexToLessIndex.RemoveSplineAt(0);

        DisplayGraphSplines(inputSplineContainer);

        GameObject finalSplineGameObject = new GameObject("Final Splines");
        finalSplineGameObject.transform.parent = transform;
        finalSplineContainer = finalSplineGameObject.AddComponent<SplineContainer>();
        finalSplineContainer.RemoveSplineAt(0);

        GetFinalSplines(inputSplineContainer, lessIndexToGreaterIndex, distanceStep);
        GetFinalSplines(inputSplineContainer, greaterIndexToLessIndex, distanceStep);
    }

    #region Helpers
    private NodeVisualization SpawnNode(Node node)
    {
        NodeVisualization nodeVisulization = Instantiate(NodePrefab, node.Position, Quaternion.identity, nodesParent.transform).GetComponent<NodeVisualization>();
        nodeVisulization.NodeVisualizationInit(node);
        return nodeVisulization;
    }

    private EdgeVisualization SpawnEdge(Edge edge)
    {
        EdgeVisualization edgeVisualization = Instantiate(EdgePrefab, edge.GetHalfWay(), Quaternion.identity, edgesParent.transform).GetComponent<EdgeVisualization>();
        edgeVisualization.EdgeVisualizationInit(edge);
        return edgeVisualization;
    }
    #endregion

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

    private void DisplayGraphSplines(SplineContainer inputSplineContainer)
    {
        Graph = new Graph(inputSplineContainer);
        foreach (Node node in Graph.Nodes)
        {
            SpawnNode(node);
        }

        foreach (Edge edge in Graph.Edges)
        {
            SpawnEdge(edge);
        }

        for (int i = 0; i < Graph.Edges.Count; i++)
        {
            Edge edge = Graph.Edges[i];
            Spline spline = new Spline(edge.GetKnots(), false);

            if (i % 2 == 0)
                lessIndexToGreaterIndex.AddSpline(spline);
            else
                greaterIndexToLessIndex.AddSpline(spline);
        }

        foreach (Spline spline in lessIndexToGreaterIndex.Splines)
            spline.SetTangentMode(TangentMode.AutoSmooth);

        foreach (Spline spline in greaterIndexToLessIndex.Splines)
            spline.SetTangentMode(TangentMode.AutoSmooth);
    }

    private void GetFinalSplines(SplineContainer inputSplineContainer, SplineContainer graphSplineContainer, float distanceStep)
    {
        HashSet<Vector3> inputPoints = GetInterpolatedSplineContainerPoints(inputSplineContainer, distanceStep);
        GameObject debugFinalDebugHolder = null;

        if (DisplayDebug)
        {
            debugFinalDebugHolder = new GameObject("Spline Debug Holder");
            debugFinalDebugHolder.transform.parent = transform;
        }

        foreach (Spline spline in graphSplineContainer.Splines)
        {
            HashSet<Vector3> splinePoints = GetInterpolatedSplinePoints(spline, distanceStep);
            HashSet<Vector3> outputSplinePoints = new HashSet<Vector3>();

            foreach (Vector3 point in splinePoints)
            {
                Vector3 nearestInputPoint = FindClosestPoint(point, inputPoints);
                Vector3 averagePoint = Vector3.Lerp(point, nearestInputPoint, 0.5f);
                outputSplinePoints.Add(averagePoint);
            }

            HashSet<Vector3> outputPoints = MakeEqualDistanced(outputSplinePoints.ToArray(), distanceStep);
            Spline newSpline = new Spline();
            GameObject debugSplineHolder = null;

            if (DisplayDebug)
            {
                debugSplineHolder = new GameObject("Final Debug Holder");
                debugSplineHolder.transform.parent = debugFinalDebugHolder.transform;
            }

            foreach (Vector3 point in outputPoints)
            {
                newSpline.Add(point);
                if (DisplayDebug)
                {
                    GameObject debugPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debugPoint.transform.position = point;
                    debugPoint.transform.parent = debugSplineHolder.transform;
                }
            }

            finalSplineContainer.AddSpline(newSpline);
        }
    }

    #region Final Spline Helpers
    private Vector3 FindClosestPoint(Vector3 inputPoint, HashSet<Vector3> points)
    {
        Vector3 closestPoint = Vector3.zero;
        float closestDistance = Mathf.Infinity;

        foreach (Vector3 point in points)
        {
            float distance = Vector3.Distance(inputPoint, point);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    private HashSet<Vector3> MakeEqualDistanced(Vector3[] originalPoints, float distanceStep)
    {
        HashSet<Vector3> equalDistancePoints = new HashSet<Vector3>();

        if (originalPoints == null || originalPoints.Length < 2)
            return equalDistancePoints;

        equalDistancePoints.Add(originalPoints[0]);
        float remainingDistance = distanceStep;

        for (int i = 1; i < originalPoints.Length; i++)
        {
            Vector3 previousPoint = originalPoints[i - 1];
            Vector3 currentPoint = originalPoints[i];
            float segmentLength = Vector3.Distance(previousPoint, currentPoint);

            while (remainingDistance < segmentLength)
            {
                float t = remainingDistance / segmentLength;
                Vector3 newPoint = Vector3.Lerp(previousPoint, currentPoint, t);
                equalDistancePoints.Add(newPoint);

                segmentLength -= remainingDistance;
                previousPoint = newPoint;
                remainingDistance = distanceStep;
            }

            remainingDistance -= segmentLength;
        }

        equalDistancePoints.Add(originalPoints[originalPoints.Length - 1]);

        return equalDistancePoints;
    }

    private HashSet<Vector3> GetInterpolatedSplinePoints(Spline spline, float distanceStep)
    {
        HashSet<Vector3> points = new HashSet<Vector3>();
        float totalLength = spline.GetLength();
        float currentLength = 0f;

        while (currentLength < totalLength)
        {
            float t = currentLength / totalLength;
            Vector3 position = spline.EvaluatePosition(t);
            points.Add(position);

            currentLength += distanceStep;
        }
        return points;
    }

    private HashSet<Vector3> GetInterpolatedSplineContainerPoints(SplineContainer splineContainer, float distanceStep)
    {
        HashSet<Vector3> points = new HashSet<Vector3>();
        foreach (Spline spline in splineContainer.Splines)
            points.AddRange(GetInterpolatedSplinePoints(spline, distanceStep));
        return points;
    }
    #endregion
}