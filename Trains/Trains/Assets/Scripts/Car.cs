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
    public bool InJunction { get; private set; }

    private Spline CurrentSpline;

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
        Debug.Log("Junction Entered!");
        InJunction = true;
        CurrentEdge = GraphGenerator.GetNextEdge(CurrentEdge);
        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);

        Vector3 newPosition = CurrentSpline.EvaluatePosition(0f);
        if (Vector3.Distance(newPosition, CurrentEdge.FromNode.Position) < Vector3.Distance(newPosition, CurrentEdge.ToNode.Position)) 
        {
            newPosition = CurrentSpline.EvaluatePosition(1f);
        }

        Rigidbody.position = newPosition;

        // Reset the car's rotation to match the new spline's direction
        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(0f));
        Vector3 up = CurrentSpline.EvaluateUpVector(0f);
        transform.rotation = Quaternion.LookRotation(forward, up);

        // Optionally reset the car's velocity
        Rigidbody.velocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        NativeSpline nativeGraph = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(nativeGraph, Rigidbody.position, out float3 nearestPoint, out float t);
        t = Mathf.Clamp01(t);
        Debug.Log(t);

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