using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class TrainTrackDrawer : ImmediateModeShapeDrawer
{
    [SerializeField] private SplineContainer splineContainer;
    [Space(10)]
    [SerializeField][Range(-5f, 5f)] private float yOffset = 0f;
    [SerializeField][Range(8, 128)] private float resolution = 75;
    [SerializeField][Range(.01f, 1f)] private float thickness = 256;
    [Space(10)]
    [SerializeField][Range(0.1f, 2.5f)] private float railDistance = 1;
    [SerializeField][Range(0.1f, 2.5f)] private float sleeperDistance = 1;
    [Space(10)]
    [SerializeField][Range(0.1f, 5f)] private float closedSplineInExtraDistance = 1;
    [SerializeField][Range(0.1f, 5f)] private float closedSplineOutExtraDistance = 1;
    [Space(10)]
    [SerializeField] private Color railColor;
    [SerializeField] private Color sleeperColor;

    public override void DrawShapes(Camera cam)
    {
        if (splineContainer == null)
            splineContainer = GetComponentInChildren<SplineContainer>();

        using (Draw.Command(cam))
        {
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
    }

    private void RenderSpline(Spline spline)
    {
        float splineLength = spline.GetLength();
        int numberOfSleepers = Mathf.FloorToInt(splineLength / sleeperDistance);

        using (PolylinePath pathLeft = new PolylinePath())
        using (PolylinePath pathRight = new PolylinePath())
        {
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

                // If `i == 1` or `i == resolution - 1`, extend in the opposite direction
                if (spline.Closed)
                {
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
                }

                pathLeft.AddPoint(leftPoint);
                pathRight.AddPoint(rightPoint);
            }

            // Draw the left and right rails
            Draw.Polyline(pathLeft, false, railColor);
            Draw.Polyline(pathRight, false, railColor);

            // Draw sleepers
            for (int i = 0; i <= numberOfSleepers; i++)
            {
                float sleeperT = CalculateTForSegment(i, splineLength, numberOfSleepers);
                DrawSleeperAtPosition(spline, sleeperT, railDistance, sleeperDistance);
            }
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

    private void DrawSleeperAtPosition(Spline spline, float t, float railDistance, float sleeperDistance)
    {
        Vector3 pointOnSpline = spline.EvaluatePosition(t);
        pointOnSpline.y = pointOnSpline.y + yOffset;

        Vector3 normal = CalculateNormalAtPoint(spline, t);

        Vector3 leftPoint = pointOnSpline + normal * railDistance;
        Vector3 rightPoint = pointOnSpline - normal * railDistance;

        Vector3 midpoint = (leftPoint + rightPoint) / 2;
        Vector3 direction = (rightPoint - leftPoint).normalized * sleeperDistance;
        Draw.Line(midpoint - direction, midpoint + direction, sleeperColor);
    }
}
