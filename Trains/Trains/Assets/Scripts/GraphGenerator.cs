using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{
    #region Create Final Graph Vars
    [Header("Create Final Splines")]
    [SerializeField] private bool DisplayDebug = false;
    [SerializeField] private bool DisplayDebugNode = false;
    [SerializeField] private bool DisplayDebugEdge = false;
    [SerializeField] private bool DisplayFinalSplineSpheres = false;
    [SerializeField][Range(.1f, 5f)] private float distanceStep = .5f;

    private SplineContainer lessIndexToGreaterIndex;
    private SplineContainer greaterIndexToLessIndex;

    private static SplineContainer GreaterThanSplineContainer;
    private static SplineContainer LessThanSplineContainer;

    private NodeVisualization NodePrefab;
    private EdgeVisualization EdgePrefab;

    private GameObject nodesParent;
    private GameObject edgesParent;
    #endregion

    #region CreateGameplay Vars
    private Junction junctionPrefab;

    private GameObject junctionHolder;
    #endregion

    private static Graph Graph;

    private void Start()
    {
        ResetGraphGenerator();

        CreateFinalSplines();
        CreateGameplay();
    }

    public void CreateGameplay()
    {
        junctionHolder = new GameObject("Junction Holder");
        junctionHolder.transform.parent = transform;

        junctionPrefab = Resources.Load<Junction>("Junction");

        foreach (Node node in Graph.Nodes)
        {
            SpawnJunction(node);
        }
    }

    public void CreateFinalSplines()
    {
        //get input
        SplineContainer inputSplineContainer = GetComponent<SplineContainer>();

        //load prefabs
        NodePrefab = Resources.Load<NodeVisualization>("Node");
        EdgePrefab = Resources.Load<EdgeVisualization>("Edge");

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
            LessThanSplineContainer.AddSpline(less);

        foreach (Spline greater in GetFinalSplines(inputSplines, greaterThanSplines, distanceStep))
            GreaterThanSplineContainer.AddSpline(greater);

        //draw debug info
        if (DisplayDebug)
        {
            if (DisplayDebugNode)
                DisplayNodeDebug(Graph);
            else
                DestroyImmediate(nodesParent.gameObject);

            if (DisplayDebugEdge)
                DisplayEdgeDebug(Graph);
            else
                DestroyImmediate(edgesParent.gameObject);
        }

        //destory temp obojects
        DestroyImmediate(lessIndexToGreaterIndex.gameObject);
        DestroyImmediate(greaterIndexToLessIndex.gameObject);
    }

    public void ResetGraphGenerator()
    {
        Graph = null;
        RemoveChildren();
        Debug.ClearDeveloperConsole();
    }

    #region CreateGameplay
    private Junction SpawnJunction(Node node)
    {
        Junction newJunction = Instantiate(junctionPrefab, node.Position, Quaternion.identity, junctionHolder.transform).GetComponent<Junction>();
        newJunction.JunctionInit(node);
        return newJunction;
    }
    #endregion

    #region Create Final Spline Methods
    #region Helpers
    void CreateObjects()
    {
        //debug info
        nodesParent = new GameObject("Nodes Parent");
        nodesParent.transform.parent = transform;

        edgesParent = new GameObject("Edges Parent");
        edgesParent.transform.parent = transform;

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
        LessThanSplineContainer = finalLessThanSplinesGameObject.AddComponent<SplineContainer>();
        LessThanSplineContainer.RemoveSplineAt(0);

        GameObject finalGreaterThanSplinesGameObject = new GameObject("Final Greater Than Splines");
        finalGreaterThanSplinesGameObject.transform.parent = transform;
        GreaterThanSplineContainer = finalGreaterThanSplinesGameObject.AddComponent<SplineContainer>();
        GreaterThanSplineContainer.RemoveSplineAt(0);
    }


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
    public void RemoveChildren()
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

    #region Spline 
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
            if (DisplayDebug && DisplayFinalSplineSpheres)
            {
                foreach (Vector3 point in outputPoints)
                {
                    debugSplineHolder = new GameObject("Final Debug Holder");
                    debugSplineHolder.transform.parent = debugFinalDebugHolder.transform;

                    if (!DisplayDebug || !DisplayFinalSplineSpheres) continue;
                    GameObject debugPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debugPoint.transform.position = point;
                    debugPoint.transform.parent = debugSplineHolder.transform;
                    DestroyImmediate(debugPoint.GetComponent<SphereCollider>());
                }
            }

            foreach (Vector3 point in outputPoints)
                newSpline.Add(point);

            splines.Add(newSpline);
        }

        return splines;
    }

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

            if (!points.Contains(position)) points.Add(position);
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
    #endregion

    #region Static Methods

    public static Edge GetInverseEdge(Edge edge) 
    {
        return Graph.GetInverseEdge(edge);
    }

    public static bool IsConnected(Edge a, Edge b) 
    {
        return Graph.IsConnected(a, b);
    }

    public static Spline GetSpline(Edge edge)
    {
        if (edge.IsLessThanEdge)
        {
            Debug.LogWarning($"GetSpline LessThan: {edge.Index} | {edge}");
            return LessThanSplineContainer.Splines[edge.Index];
        }
        Debug.LogWarning($"GetSpline GreaterThan: {edge.Index} | {edge}");
        return GreaterThanSplineContainer.Splines[edge.Index];
    }

    public static Edge GetEdge(int index)
    {
        return Graph.Edges[index];
    }

    public static Edge GetNextEdge(Edge edge)
    {
        Graph.EdgeConnectionMap.TryGetValue(edge, out List<Edge> result);
        if (result == null) return null;

        Edge outputEdge = result[Random.Range(0, result.Count - 1)];
        return outputEdge;
    }
    #endregion
}