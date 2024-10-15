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
    [SerializeField][Range(0.1f, 5f)] private float closedSplineInExtraDistance = 1;
    [SerializeField][Range(0.1f, 5f)] private float closedSplineOutExtraDistance = 1;
    [Space(10)]
    [SerializeField] private Color railColor;
    [SerializeField] private Color sleeperColor;

    private Polyline railPrefab;
    private Line sleeperPrefab;

    private SplineContainer splineContainer;

    private GameObject trackParent;
    private GameObject sleepersParent;
    private GameObject railsParent;

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

        for (int i = 0; i <= resolution; i++)
        {
            if (spline.Closed && i <= 0 || i >= resolution) continue;

            float t = CalculateTForSegment(i, splineLength, (int)resolution);
            Vector3 pointOnSpline = spline.EvaluatePosition(t);
            Vector3 tangent = spline.EvaluateTangent(t);
            Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;

            pointOnSpline.y = pointOnSpline.y + yOffset;

            Vector3 leftPoint = pointOnSpline + normal * railDistance;
            Vector3 rightPoint = pointOnSpline - normal * railDistance;

            Vector3 oppositeTangent = -tangent.normalized * sleeperDistance;
            Vector3 backLeftPoint = leftPoint;
            Vector3 backRightPoint = rightPoint;

            if (i == 1)
            {
                leftPoint += oppositeTangent * railDistance * closedSplineInExtraDistance;
                rightPoint += oppositeTangent * railDistance * closedSplineInExtraDistance;
            }

            if (i == resolution - 1)
            {
                leftPoint -= oppositeTangent * railDistance * closedSplineOutExtraDistance;
                rightPoint -= oppositeTangent * railDistance * closedSplineOutExtraDistance;
            }

            leftRail.AddPoint(leftPoint);
            rightRail.AddPoint(rightPoint);
        }

        sleepersParent = new GameObject("Sleeprs");
        sleepersParent.transform.parent = trackParent.transform;

        // Draw sleepers
        for (int i = 0; i <= numberOfSleepers; i++)
        {
            float sleeperT = CalculateTForSegment(i, splineLength, numberOfSleepers);

            Vector3 pointOnSpline = spline.EvaluatePosition(sleeperT);
            pointOnSpline.y = pointOnSpline.y + yOffset;

            Vector3 normal = CalculateNormalAtPoint(spline, sleeperT);

            Vector3 leftPoint = pointOnSpline + normal * sleeperDistance;
            Vector3 rightPoint = pointOnSpline - normal * sleeperDistance;

            Vector3 midpoint = (leftPoint + rightPoint) / 2;
            Vector3 direction = (rightPoint - leftPoint).normalized * sleeperLength;

            Line line = Instantiate(sleeperPrefab, Vector3.up * yOffset, Quaternion.identity, sleepersParent.transform);
            line.Color = sleeperColor;
            line.Thickness = thickness;

            line.Start = midpoint - direction;
            line.End = midpoint + direction;
        }
    }

    private float CalculateTForSegment(int index, float splineLength, int segments)
    {
        float distance = index * (splineLength / segments);
        return distance / splineLength;
    }

    private Vector3 CalculateNormalAtPoint(Spline spline, float t)
    {
        Vector3 tangent = spline.EvaluateTangent(t);
        return Vector3.Cross(tangent, Vector3.up).normalized;
    }
}
