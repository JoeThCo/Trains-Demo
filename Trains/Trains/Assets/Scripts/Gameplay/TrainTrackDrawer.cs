
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Shapes;

public class TrainTrackDrawer : MonoBehaviour
{
    [SerializeField][Range(-5f, 5f)] private float yOffset = 0f;
    [SerializeField][Range(8, 128)] private float resolution = 75;
    [SerializeField][Range(.01f, 1f)] private float thickness = 256;
    [Space(10)]
    [SerializeField][Range(0.1f, 2.5f)] private float railDistance = 1;
    [Space(10)]
    [SerializeField][Range(0.1f, 2.5f)] private float sleeperDistance = 1;
    [SerializeField][Range(0.1f, 2.5f)] private float sleeperLength = 1;
    [Space(10)]
    [SerializeField] private Color railColor;
    [SerializeField] private Color sleeperColor;

    private Polyline railPrefab;
    private Line sleeperPrefab;

    private SplineContainer splineContainer;

    private GameObject trackParent;
    private GameObject sleepersParent;
    private GameObject railsParent;

    private void Start()
    {
        DrawTracksAndSleepers();
    }

    public void DrawTracksAndSleepers()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        if (railPrefab == null)
            railPrefab = Resources.Load<Polyline>("RailPrefab");

        if (sleeperPrefab == null)
            sleeperPrefab = Resources.Load<Line>("SleeperPrefab");

        if (trackParent == null)
        {
            trackParent = new GameObject("Track Parent");
            trackParent.transform.parent = transform;
        }

        // Set up line drawing parameters
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.ThicknessSpace = ThicknessSpace.Meters;
        Draw.Thickness = thickness;

        // Set the matrix to draw in world space
        Draw.Matrix = Matrix4x4.identity;

        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            Spline spline = splineContainer.Splines[i];
            RenderSpline(spline);
        }
    }

    private void RenderSpline(Spline spline)
    {
        float splineLength = spline.GetLength();
        int numberOfSleepers = Mathf.FloorToInt(splineLength / sleeperDistance);

        GameObject rails = new GameObject("Rails");
        rails.transform.parent = trackParent.transform;

        Polyline leftRail = Instantiate(railPrefab, Vector3.up * yOffset, Quaternion.identity, rails.transform);
        leftRail.Color = railColor;
        leftRail.Thickness = thickness;

        Polyline rightRail = Instantiate(railPrefab, Vector3.up * yOffset, Quaternion.identity, rails.transform);
        rightRail.Color = railColor;
        rightRail.Thickness = thickness;

        List<Vector3> leftPoints = new List<Vector3>();
        List<Vector3> rightPoints = new List<Vector3>();

        // Calculate points for the entire spline
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector3 pointOnSpline = spline.EvaluatePosition(t);
            Vector3 tangent;

            // Special handling for endpoints on closed splines
            if (spline.Closed && (t == 0f || t == 1f))
            {
                // For t=0, use forward difference
                if (t == 0f)
                {
                    Vector3 nextPoint = spline.EvaluatePosition(t + 0.01f);
                    tangent = (nextPoint - pointOnSpline).normalized;
                }
                // For t=1, use backward difference and extend one unit forward
                else
                {
                    Vector3 prevPoint = spline.EvaluatePosition(t - 0.01f);
                    tangent = (pointOnSpline - prevPoint).normalized;

                    // Extend one unit forward from the end point
                    pointOnSpline += tangent * railDistance;
                }
            }
            else
            {
                tangent = spline.EvaluateTangent(t);

                // Fallback for zero tangents
                if (tangent.magnitude < 0.001f)
                {
                    Vector3 prevPoint = spline.EvaluatePosition(t - 0.01f);
                    Vector3 nextPoint = spline.EvaluatePosition(t + 0.01f);
                    tangent = (nextPoint - prevPoint).normalized;
                }
            }

            Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;
            pointOnSpline.y += yOffset;

            leftPoints.Add(pointOnSpline + normal * railDistance);
            rightPoints.Add(pointOnSpline - normal * railDistance);
        }

        // For closed splines, ensure the rails don't match up at the end
        if (spline.Closed)
        {
            // Get the tangent at the end (t=1)
            Vector3 endPoint = spline.EvaluatePosition(1f);
            Vector3 endTangent = spline.EvaluateTangent(1f);

            // Fallback for zero tangent
            if (endTangent.magnitude < 0.001f)
            {
                Vector3 prevPoint = spline.EvaluatePosition(0.99f);
                endTangent = (endPoint - prevPoint).normalized;
            }

            // Extend the end points one rail distance forward
            Vector3 extendedEndPoint = endPoint + endTangent * railDistance;
            Vector3 endNormal = Vector3.Cross(endTangent, Vector3.up).normalized;

            // Replace the last points with extended versions
            leftPoints[leftPoints.Count - 1] = extendedEndPoint + endNormal * railDistance;
            rightPoints[rightPoints.Count - 1] = extendedEndPoint - endNormal * railDistance;
        }

        // Add all points to the rails
        for (int i = 0; i < leftPoints.Count; i++)
        {
            leftRail.AddPoint(leftPoints[i]);
            rightRail.AddPoint(rightPoints[i]);
        }

        // Sleepers drawing (with similar endpoint handling)
        sleepersParent = new GameObject("Sleepers");
        sleepersParent.transform.parent = trackParent.transform;

        for (int i = 0; i <= numberOfSleepers; i++)
        {
            float sleeperT = (float)i / numberOfSleepers;
            Vector3 pointOnSpline = spline.EvaluatePosition(sleeperT);
            Vector3 tangent;

            // Handle endpoints for closed splines
            if (spline.Closed && (sleeperT == 0f || sleeperT == 1f))
            {
                if (sleeperT == 0f)
                {
                    Vector3 nextPoint = spline.EvaluatePosition(sleeperT + 0.01f);
                    tangent = (nextPoint - pointOnSpline).normalized;
                }
                else
                {
                    Vector3 prevPoint = spline.EvaluatePosition(sleeperT - 0.01f);
                    tangent = (pointOnSpline - prevPoint).normalized;
                    pointOnSpline += tangent * railDistance;
                }
            }
            else
            {
                tangent = spline.EvaluateTangent(sleeperT);

                if (tangent.magnitude < 0.001f)
                {
                    Vector3 prevPoint = spline.EvaluatePosition(sleeperT - 0.01f);
                    Vector3 nextPoint = spline.EvaluatePosition(sleeperT + 0.01f);
                    tangent = (nextPoint - prevPoint).normalized;
                }
            }

            Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;
            pointOnSpline.y += yOffset;

            Vector3 leftPoint = pointOnSpline + normal * railDistance;
            Vector3 rightPoint = pointOnSpline - normal * railDistance;

            Vector3 midpoint = (leftPoint + rightPoint) * 0.5f;
            Vector3 direction = (rightPoint - leftPoint).normalized * sleeperLength;

            Line line = Instantiate(sleeperPrefab, Vector3.up * yOffset, Quaternion.identity, sleepersParent.transform);
            line.Color = sleeperColor;
            line.Thickness = thickness;

            line.Start = midpoint - direction;
            line.End = midpoint + direction;
        }
    }
}