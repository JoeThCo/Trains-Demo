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
    public Edge DirectionEdge { get; private set; }
    private Spline CurrentSpline { get; set; }

    [SerializeField] private float gizmoLineDistance = 7.5f;

    private float t = 0;
    private float dot = 0;

    private bool isSwitching = false;
    private bool isForward = true;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();

        OnEdgeChanged(GraphGenerator.GetEdge(0));
        Debug.Log(DirectionEdge);

        Rigidbody.position = CurrentEdge.GetHalfWay();
        transform.rotation = Quaternion.Euler(CurrentEdge.EdgeDireciton);
        UpdateCarTransform();

        Debug.LogWarning($"Edge: {CurrentEdge.Index}");
    }

    private void FixedUpdate()
    {
        UpdateCarTransform();

        if (!isSwitching && IsAtEndOfSpline())
        {
            isSwitching = true;
            OnJunctionEnter();
        }

        if ((isForward && dot < 0) || (!isForward && dot > 0))
        {
            isForward = !isForward;
            DirectionEdge = GraphGenerator.GetInverse(DirectionEdge);
            Debug.Log(DirectionEdge);
        }
    }

    public void Throttle(float power)
    {
        Rigidbody.AddForce(transform.forward * power);
    }

    public void SetTrain(Train train)
    {
        Train = train;
    }

    public void OnJunctionEnter()
    {
        Debug.Log($"Forward: {isForward} Edge: {DirectionEdge}");
        Edge nextEdge = GraphGenerator.GetNextEdge(DirectionEdge);

        if (nextEdge == null) return;
        OnEdgeChanged(nextEdge);

        ResetVelocity();
        UpdateCarTransform();
    }

    private void ResetVelocity()
    {
        float speed = Rigidbody.velocity.magnitude;
        if (speed < 0.1f)
            speed = 0;
        Rigidbody.velocity = isForward ? transform.forward * speed : transform.forward * -speed;
    }

    private void OnEdgeChanged(Edge edge)
    {
        CurrentEdge = edge;
        DirectionEdge = CurrentEdge;

        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
    }

    public void OnJunctionExit()
    {
        isSwitching = false;
    }

    void UpdateCarTransform()
    {
        NativeSpline nativeSpline = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(new NativeSpline(CurrentSpline), Rigidbody.position, out float3 nearestPoint, out float newT);
        t = Mathf.Clamp01(newT);

        Rigidbody.position = (Vector3)nearestPoint;

        Vector3 splineForward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));
        Vector3 forward = CurrentEdge.IsLessThanEdge ? splineForward : -splineForward;
        Vector3 up = nativeSpline.EvaluateUpVector(t);

        transform.rotation = Quaternion.LookRotation(forward, up);
        Vector3 engineForward = transform.forward;

        dot = GetDirectionDot();
        if (dot < 0)
        {
            engineForward *= -1;
        }

        Rigidbody.velocity = Rigidbody.velocity.magnitude * engineForward;
    }

    bool IsAtEndOfSpline()
    {
        return t <= .01 || t >= .99;
    }

    private float GetDirectionDot()
    {
        return Vector3.Dot(Rigidbody.velocity, transform.forward);
    }

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