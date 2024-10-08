using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    public Train Train { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Edge CurrentEdge { get; private set; }
    private Spline CurrentSpline { get; set; }

    private Vector3 wantedPosition;
    private Quaternion wantedRotation;

    [SerializeField] private float gizmoLineDistance = 7.5f;

    private float t = 0;
    private float dot = 0;

    private bool isSwitching = false;
    private bool isForward = true;
    private bool isRotationBackwards = false;

    private const float ENTER_EPSILON = .005f;

    private const int POSITION_LERP_SPEED = 20;
    private const int ROTATION_LERP_SPEED = 15;

    private void Start()
    {
        CarInit();
    }

    protected virtual void CarInit()
    {
        Rigidbody = GetComponent<Rigidbody>();

        OnEdgeChanged(GraphGenerator.GetEdge(0));

        Rigidbody.position = CurrentEdge.MidPoint;
        transform.rotation = Quaternion.Euler(CurrentEdge.EdgeDireciton);
        FixedUpdate();

        Debug.LogWarning($"Edge: {CurrentEdge.Index}");
    }

    protected virtual void FixedUpdate()
    {
        dot = GetDirectionDot();
        UpdateCarTransform();

        Rigidbody.position = Vector3.Lerp(Rigidbody.position, wantedPosition, POSITION_LERP_SPEED * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, ROTATION_LERP_SPEED * Time.fixedDeltaTime);

        if (!isSwitching && IsAtEndOfSpline())
        {
            isSwitching = true;
            OnJunctionEnter();
        }

        if ((isForward && dot < 0) || (!isForward && dot > 0))
        {
            isForward = !isForward;
        }
    }

    public void SetTrain(Train train)
    {
        Train = train;
    }

    #region Junctions
    public void OnJunctionEnter()
    {
        Edge inputEdge = CurrentEdge;

        if (!isForward && !isRotationBackwards || isForward && isRotationBackwards)
        {
            Debug.Log($"isRotationBackwards Flipped! {isRotationBackwards}");
            isRotationBackwards = !isRotationBackwards;
            inputEdge = GraphGenerator.GetInverse(CurrentEdge);
        }

        Edge nextEdge = GraphGenerator.GetNextEdge(inputEdge);
        if (nextEdge == null)
        {
            OnDeadEndHit();
            return;
        }

        OnEdgeChanged(nextEdge);
        UpdateCarTransform();
    }

    private void OnDeadEndHit()
    {
        Rigidbody.velocity = Vector3.zero;
    }

    private void OnEdgeChanged(Edge edge)
    {
        CurrentEdge = edge;
        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
    }

    public void OnJunctionExit()
    {
        isSwitching = false;
    }
    #endregion

    #region Car Transform
    private void UpdateCarTransform()
    {
        NativeSpline nativeSpline = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(nativeSpline, Rigidbody.position, out float3 nearestPoint, out float newT);
        t = Mathf.Clamp01(newT);

        wantedPosition = (Vector3)nearestPoint;
        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));
        Vector3 up = nativeSpline.EvaluateUpVector(t);

        wantedRotation = isRotationBackwards ? Quaternion.LookRotation(-forward, up) : Quaternion.LookRotation(forward, up);

        Vector3 engineForward = wantedRotation * Vector3.forward;
        if (dot < 0)
        {
            engineForward *= -1;
        }
        Rigidbody.velocity = Rigidbody.velocity.magnitude * engineForward;
    }

    private bool IsAtEndOfSpline()
    {
        return t <= 0 + ENTER_EPSILON || t >= 1 - ENTER_EPSILON;
    }

    private float GetDirectionDot()
    {
        return Vector3.Dot(Rigidbody.velocity, transform.forward);
    }
    #endregion

    void OnDrawGizmos()
    {
        if (Rigidbody == null) return;
        if (CurrentSpline == null) return;

        Vector3 velocity = Rigidbody.velocity.normalized;
        Vector3 position = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + (velocity * gizmoLineDistance));

        NativeSpline nativeSpline = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(new NativeSpline(CurrentSpline), Rigidbody.position, out float3 nearestPoint, out float newT);
        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + (forward * gizmoLineDistance));
    }
}