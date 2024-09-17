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
    [SerializeField] private SplineContainer rail;

    private Spline currentSpline;
    public const float MAX_JUNCTION_ANGLE = 30f;

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        currentSpline = rail.Splines[0];
    }

    public void Throttle(float power)
    {
        Rigidbody.AddForce(transform.forward * power);
    }

    public void SetTrain(Train train)
    {
        Train = train;
    }

    public void SetSpline(Spline spline)
    {
        currentSpline = spline;
    }

    //TODO put in methods
    private void FixedUpdate()
    {
        //get spline information
        NativeSpline native = new NativeSpline(currentSpline);
        SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest, out float t);

        //set to nearest point
        transform.position = new float3(nearest.x, rail.transform.position.y, nearest.z);

        //get the new directions
        Vector3 forward = Vector3.Normalize(native.EvaluateTangent(t));
        Vector3 up = native.EvaluateUpVector(t);

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

    public float GetDirectionDot()
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