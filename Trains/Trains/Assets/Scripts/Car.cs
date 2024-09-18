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

    [SerializeField] private SplineContainer graphSpline;

    public const float MAX_JUNCTION_ANGLE = 30f;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
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
        NativeSpline nativeGraph = new NativeSpline(graphSpline.Splines[0]);
        SplineUtility.GetNearestPoint(nativeGraph, transform.position, out float3 nearestGraph, out float t);

        //set to nearest point
        Rigidbody.position = (Vector3)nearestGraph;

        //get the new directions
        Vector3 forward = Vector3.Normalize(nativeGraph.EvaluateTangent(t));
        Vector3 up = nativeGraph.EvaluateUpVector(t);

        //set the new rotation
        transform.rotation = Quaternion.LookRotation(forward, up) * Quaternion.Inverse(Quaternion.LookRotation(Vector3.forward, Vector3.up));

        //check if going backwards
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