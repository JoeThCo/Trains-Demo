using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField][Range(1f, 10f)] private float DistanceStep = 5;
    [SerializeField] private bool DisplayDebugSpheres = false;
    [SerializeField] private bool DisplayGraphEdges = false;

    public static SplineContainer InputSplineContainer;
    private static SplineContainer FinalSplineContainer;

    private EdgeVisualization EdgePrefab;

    private GameObject graphEdgesParent;
    private GameObject finalSplinesSphereParent;

    private Junction junctionPrefab;
    private GameObject junctionHolder;

    private static Graph Graph;

    private void Start()
    {
        CreateGraph();
    }

    public void CreateGraph()
    {
        ResetGraph();

        InputSplineContainer = GetComponent<SplineContainer>();
        Graph = new Graph(InputSplineContainer);

        GameObject finalSplinesParent = new GameObject("Final Splines");
        finalSplinesParent.transform.parent = transform;
        FinalSplineContainer = finalSplinesParent.AddComponent<SplineContainer>();
        FinalSplineContainer.RemoveSplineAt(0);

        graphEdgesParent = new GameObject("Graph Edges Parent");
        graphEdgesParent.transform.parent = transform;

        finalSplinesSphereParent = new GameObject("Final Splines Sphere Parent");
        finalSplinesSphereParent.transform.parent = transform;

        Spline[] finalSplines = GetFinalSplines(GetSplinesFromEdge(Graph), DistanceStep);
        foreach (Spline spline in finalSplines)
            FinalSplineContainer.AddSpline(spline);

        if (DisplayGraphEdges)
            DebugEdgeInfo();
        else
            DestroyImmediate(graphEdgesParent.gameObject);

        if (DisplayDebugSpheres)
            DebugFinalSplineInfo(finalSplines);
        else
            DestroyImmediate(finalSplinesSphereParent.gameObject);

        CreateGameplay();
    }


    #region Gameplay
    private void CreateGameplay()
    {
        junctionHolder = new GameObject("Junction Holder");
        junctionHolder.transform.parent = transform;

        junctionPrefab = Resources.Load<Junction>("Junction");

        foreach (Node node in Graph.Nodes)
        {
            SpawnJunction(node);
        }
    }

    private Junction SpawnJunction(Node node)
    {
        Junction newJunction = Instantiate(junctionPrefab, node.Position, Quaternion.identity, junctionHolder.transform).GetComponent<Junction>();
        newJunction.JunctionInit(node);
        return newJunction;
    }
    #endregion

    #region Splines
    private Spline[] GetSplinesFromEdge(Graph graph)
    {
        List<Spline> splines = new List<Spline>();
        for (int i = 0; i < graph.Edges.Length; i++)
        {
            Edge edge = graph.Edges[i];
            Spline spline = new Spline(edge.Knots, false);
            spline.SetTangentMode(TangentMode.AutoSmooth);
            splines.Add(spline);
        }
        return splines.ToArray();
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

    public static Spline GetNearestSpline(Vector3 position, SplineContainer splineContainer)
    {
        float minDistance = float.MaxValue;
        Spline nearestSpline = null;

        foreach (Spline spline in splineContainer.Splines)
        {
            float3 closestPointOnSpline;
            float t;

            SplineUtility.GetNearestPoint(spline, position, out closestPointOnSpline, out t);
            float distance = Vector3.Distance(position, closestPointOnSpline);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestSpline = spline;
            }
        }
        return nearestSpline;
    }

    private Spline[] GetFinalSplines(Spline[] graphSplines, float distanceStep)
    {
        List<Spline> splines = new List<Spline>();

        foreach (Spline spline in graphSplines)
        {
            List<Vector3> splinePoints = GetInterpolatedSplinePoints(spline, distanceStep);
            HashSet<Vector3> outputSplinePoints = new HashSet<Vector3>() { splinePoints[0] };
            foreach (Vector3 point in splinePoints)
            {
                NativeSpline nativeSpline = new NativeSpline(GetNearestSpline(point, InputSplineContainer));
                SplineUtility.GetNearestPoint(nativeSpline, point, out float3 nearest, out float t);
                outputSplinePoints.Add(nearest);
            }
            outputSplinePoints.Add(splinePoints[splinePoints.Count - 1]);

            HashSet<Vector3> outputPoints = MakeEqualDistanced(outputSplinePoints.ToArray(), distanceStep);
            Spline newSpline = new Spline();
            foreach (Vector3 point in outputPoints)
                newSpline.Add(point);
            splines.Add(newSpline);
        }
        return splines.ToArray();
    }
    #endregion

    #region Debug
    public void ResetGraph()
    {
        Graph = null;
        RemoveChildren();
        Debug.ClearDeveloperConsole();
    }

    private void DebugEdgeInfo()
    {
        EdgePrefab = Resources.Load<EdgeVisualization>("Edge");
        DisplayEdgeDebug(Graph);
    }

    private void DebugFinalSplineInfo(Spline[] finalSplines)
    {
        foreach (Spline spline in finalSplines)
        {
            GameObject currentSplineDebugHolder = null;
            currentSplineDebugHolder = new GameObject("Current Spline Holder");
            currentSplineDebugHolder.transform.parent = finalSplinesSphereParent.transform;

            foreach (BezierKnot knot in spline.Knots)
            {
                GameObject debugPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugPoint.name = "Final Debug Holder";
                debugPoint.transform.position = knot.Position;
                DestroyImmediate(debugPoint.GetComponent<SphereCollider>());
                debugPoint.transform.parent = currentSplineDebugHolder.transform;
            }
        }
    }

    private EdgeVisualization SpawnEdge(Edge edge)
    {
        EdgeVisualization edgeVisualization = Instantiate(EdgePrefab, edge.MidPoint, Quaternion.identity, graphEdgesParent.transform).GetComponent<EdgeVisualization>();
        edgeVisualization.EdgeVisualizationInit(edge);
        return edgeVisualization;
    }

    private void RemoveChildren()
    {
        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void DisplayEdgeDebug(Graph graph)
    {
        foreach (Edge edge in graph.Edges)
            SpawnEdge(edge);
    }
    #endregion

    #region Static Methods
    public static Spline GetSpline(Edge edge)
    {
        return FinalSplineContainer.Splines[edge.Index];
    }

    public static Edge GetEdge(int index)
    {
        return Graph.Edges[index];
    }

    public static Edge GetInverse(Edge edge)
    {
        return Graph.Edges[edge.InverseIndex];
    }

    public static Edge[] GetConnections(Edge edge)
    {
        Graph.EdgeConnectionMap.TryGetValue(edge, out List<Edge> output);
        if (output == null || output.Count <= 0)
            return null;
        return output.ToArray();
    }

    public static Edge GetNextEdge(Edge edge)
    {
        //Debug.LogWarning($"In {edge}");
        Edge[] result = GetConnections(edge);
        if (result == null) return null;
        //foreach (Edge mapEdges in result)
        //Debug.LogError(mapEdges);

        int randomIndex = Graph.Random.Next(0, result.Length);
        Edge outputEdge = result[randomIndex];
        //Debug.LogWarning($"Out {outputEdge}");
        return outputEdge;
    }
    #endregion
}