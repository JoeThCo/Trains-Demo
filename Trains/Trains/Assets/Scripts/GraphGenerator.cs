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

    private SplineContainer finalLessThanSplinesContainer;
    private SplineContainer finalGreaterThanSplinesContainer;

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

        CreateObjects();

        //get edge/node info
        Graph = new Graph(inputSplineContainer);

        //get input splines
        List<Spline> inputSplines = inputSplineContainer.Splines.ToList();

        //get graph splines
        List<Spline> lessThanSplines = GetSplinesFromEdge(Graph, true);
        List<Spline> greaterThanSplines = GetSplinesFromEdge(Graph, false);

        //add final splines to correct container
        foreach (Spline less in GetFinalSplines(inputSplines, lessThanSplines, distanceStep))
            finalLessThanSplinesContainer.AddSpline(less);

        foreach (Spline greater in GetFinalSplines(inputSplines, greaterThanSplines, distanceStep))
            finalGreaterThanSplinesContainer.AddSpline(greater);

        //draw debug info
        if (DisplayDebug)
        {
            DisplayNodeDebug(Graph);
            DisplayEdgeDebug(Graph);
        }
        else 
        {
            DestroyImmediate(nodesParent.gameObject);
            DestroyImmediate(edgesParent.gameObject);
        }

        //destory temp obojects
        DestroyImmediate(lessIndexToGreaterIndex.gameObject);
        DestroyImmediate(greaterIndexToLessIndex.gameObject);
    }

    void CreateObjects()
    {
        //First passs output slines
        GameObject FromToToGameObject = new GameObject("From -> To Splines");
        FromToToGameObject.transform.parent = transform;
        lessIndexToGreaterIndex = FromToToGameObject.AddComponent<SplineContainer>();
        lessIndexToGreaterIndex.RemoveSplineAt(0);

        GameObject ToToFromGameObject = new GameObject("To -> From Splines");
        ToToFromGameObject.transform.parent = transform;
        greaterIndexToLessIndex = ToToFromGameObject.AddComponent<SplineContainer>();
        greaterIndexToLessIndex.RemoveSplineAt(0);

        //final output slines
        GameObject finalLessThanSplinesGameObject = new GameObject("Final Less Than Splines");
        finalLessThanSplinesGameObject.transform.parent = transform;
        finalLessThanSplinesContainer = finalLessThanSplinesGameObject.AddComponent<SplineContainer>();
        finalLessThanSplinesContainer.RemoveSplineAt(0);

        GameObject finalGreaterThanSplinesGameObject = new GameObject("Final Greater Than Splines");
        finalGreaterThanSplinesGameObject.transform.parent = transform;
        finalGreaterThanSplinesContainer = finalGreaterThanSplinesGameObject.AddComponent<SplineContainer>();
        finalGreaterThanSplinesContainer.RemoveSplineAt(0);
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

    #region Debug
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

    private void DisplayNodeDebug(Graph graph)
    {
        foreach (Node node in graph.Nodes)
        {
            SpawnNode(node);
        }
    }

    private void DisplayEdgeDebug(Graph graph)
    {
        foreach (Edge edge in graph.Edges)
        {
            SpawnEdge(edge);
        }
    }
    #endregion

    private List<Spline> GetSplinesFromEdge(Graph graph, bool wantLessThan)
    {
        List<Spline> splines = new List<Spline>();

        for (int i = 0; i < graph.Edges.Count; i++)
        {
            Edge edge = graph.Edges[i];
            Spline spline = new Spline(edge.GetKnots(), false);
            spline.SetTangentMode(TangentMode.AutoSmooth);

            if (wantLessThan && i % 2 == 0)
                splines.Add(spline);

            if (!wantLessThan && i % 2 != 0)
                splines.Add(spline);
        }

        return splines;
    }

    private List<Spline> GetFinalSplines(List<Spline> inputSplines, List<Spline> graphSlines, float distanceStep)
    {
        List<Spline> splines = new List<Spline>();
        HashSet<Vector3> inputPoints = GetInterpolatedSplineContainerPoints(inputSplines, distanceStep);
        GameObject debugFinalDebugHolder = null;

        if (DisplayDebug)
        {
            debugFinalDebugHolder = new GameObject("Spline Debug Holder");
            debugFinalDebugHolder.transform.parent = transform;
        }

        foreach (Spline spline in graphSlines)
        {
            List<Vector3> splinePoints = GetInterpolatedSplinePoints(spline, distanceStep);
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
                
                if (!DisplayDebug) continue;
                GameObject debugPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugPoint.transform.position = point;
                debugPoint.transform.parent = debugSplineHolder.transform;
                DestroyImmediate(debugPoint.GetComponent<SphereCollider>());
            }

            splines.Add(newSpline);
        }

        return splines;
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

    private List<Vector3> GetInterpolatedSplinePoints(Spline spline, float distanceStep)
    {
        List<Vector3> points = new List<Vector3>();
        float totalLength = spline.GetLength();
        float currentLength = 0f;
        Vector3 position = spline.EvaluatePosition(0);
        points.Add(position);

        while (currentLength < totalLength)
        {
            float t = currentLength / totalLength;
            position = spline.EvaluatePosition(t);
            
            if(!points.Contains(position)) points.Add(position);
            currentLength += distanceStep;
        }
        points.Add(spline.EvaluatePosition(1));

        return points;
    }

    private HashSet<Vector3> GetInterpolatedSplineContainerPoints(List<Spline> splines, float distanceStep)
    {
        HashSet<Vector3> points = new HashSet<Vector3>();
        foreach (Spline spline in splines)
            points.AddRange(GetInterpolatedSplinePoints(spline, distanceStep));
        return points;
    }
    #endregion
}