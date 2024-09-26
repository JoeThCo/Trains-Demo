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

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        CurrentEdge = GraphGenerator.GetEdge(0);
        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
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
        Edge nextEdge = GraphGenerator.GetNextEdge(CurrentEdge);
        if (nextEdge == null) return;

        if (!GraphGenerator.IsConnected(CurrentEdge, nextEdge))
        {
            CurrentEdge = GraphGenerator.GetInverseEdge(CurrentEdge);
            CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
            Rigidbody.velocity = Vector3.zero;
            return;
        }
        else
        {
            CurrentEdge = nextEdge;
        }

        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);

        NativeSpline nativeGraph = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(nativeGraph, Rigidbody.position, out float3 nearestPoint, out float t);
        t = Mathf.Clamp01(t);

        // Set to nearest point
        Rigidbody.position = (Vector3)nearestPoint;

        // Get the new directions
        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));
        Vector3 up = nativeGraph.EvaluateUpVector(t);

        // Set the new rotation
        transform.rotation = Quaternion.LookRotation(forward, up);
    }

    private void FixedUpdate()
    {
        float dot = GetDirectionDot();

        NativeSpline nativeGraph = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(nativeGraph, Rigidbody.position, out float3 nearestPoint, out float t);
        t = Mathf.Clamp01(t);
        //Debug.Log(t);

        // Set to nearest point
        Rigidbody.position = (Vector3)nearestPoint;

        // Get the new directions
        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));
        Vector3 up = nativeGraph.EvaluateUpVector(t);

        // Set the new rotation
        transform.rotation = Quaternion.LookRotation(forward, up);

        // Check if going backwards
        Vector3 engineForward = transform.forward;
        if (GetDirectionDot() < 0)
        {
            engineForward *= -1;
        }

        Rigidbody.velocity = Rigidbody.velocity.magnitude * engineForward;
    }

    private float GetDirectionDot()
    {
        return Vector3.Dot(Rigidbody.velocity, transform.forward);
    }

    void OnDrawGizmos()
    {
        if (Rigidbody == null) return;

        Vector3 velocity = Rigidbody.velocity.normalized;
        Vector3 position = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + velocity);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + transform.forward);
    }
}