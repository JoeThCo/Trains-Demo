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

    public bool IsSwitching { get; set; }

    private const float JUNCTION_DISTANCE = .05f;

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

    private void FixedUpdate()
    {
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

        if (t <= JUNCTION_DISTANCE || t >= 1 - JUNCTION_DISTANCE)
        {
            if (IsSwitching) return;
            IsSwitching = true;

            Edge nextEdge = GraphGenerator.GetNextEdge(CurrentEdge);
            if (nextEdge == null) return;

            if (!GraphGenerator.IsConnected(CurrentEdge, nextEdge))
            {
                Debug.LogWarning("Not Conneceted!");
                Rigidbody.velocity = Vector3.zero;
                return;
            }
            else
            {
                Debug.LogWarning("Is Conneceted!");
                CurrentEdge = nextEdge;
            }

            CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
        }
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
        Gizmos.DrawLine(position, position + (velocity * 7.5f));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + (transform.forward * 7.5f));
    }
}