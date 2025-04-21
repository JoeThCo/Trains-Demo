using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    public Rigidbody Rigidbody { get; private set; }
    public Edge CurrentEdge { get; private set; }
    private Spline CurrentSpline { get; set; }
    public Vector3 WantedPosition { get; private set; }
    public Quaternion WantedRotation { get; private set; }
    public AttachToCar AttachToCar { get; private set; }
    public Train Train { get; private set; }
    public int Index { get; private set; }

    [Header("Car")]
    [Range(0, 500)][SerializeField] private int positionLerpSpeed = 15;
    [Range(0, 500)][SerializeField] private int rotationLerpSpeed = 15;
    [Space(10)]
    [SerializeField] private float gizmoLineDistance = 7.5f;

    private float t = 0;
    private float forwardT = 0;
    private float backwardT = 0;
    public float Dot { get; private set; }

    private bool isSwitching = false;
    private bool isForward = true;
    private bool isRotationBackwards = false;

    public const float JUNCTION_EPSILON = .005f;

    public delegate void EdgeChanged(Edge edge);
    public delegate void JunctionAction();

    public event EdgeChanged OnEdgeChanged;
    public event JunctionAction OnJunctionEnter;
    public event JunctionAction OnJunctionExit;
    public event JunctionAction OnDeadEnd;

    private const int LERP_SPEED = 250;

    private void Start()
    {
        OnEdgeChanged += Car_OnEdgeChanged;

        OnJunctionEnter += Car_OnJunctionEnter;
        OnJunctionExit += Car_OnJunctionExit;
        OnDeadEnd += Car_OnDeadEnd;

        CarInit();
    }

    private void OnDisable()
    {
        OnEdgeChanged -= Car_OnEdgeChanged;

        OnJunctionEnter -= Car_OnJunctionEnter;
        OnJunctionExit -= Car_OnJunctionExit;
        OnDeadEnd -= Car_OnDeadEnd;
    }

    protected virtual void CarInit()
    {
        Rigidbody = GetComponent<Rigidbody>();
        AttachToCar = GetComponentInChildren<AttachToCar>();

        Car_OnEdgeChanged(GraphGenerator.GetEdge(0));
        //Debug.LogWarning($"Edge: {CurrentEdge.Index}");

        FixedUpdate();

        Rigidbody.MovePosition(WantedPosition);
        Rigidbody.MoveRotation(WantedRotation);

        Rigidbody.velocity = Vector3.zero;
    }

    public void SetTrain(Train train)
    {
        this.Train = train;
    }

    public void SetTrainIndex(int index)
    {
        this.Index = index;
    }

    protected virtual void FixedUpdate()
    {
        Dot = Vector3.Dot(Rigidbody.velocity, transform.forward);
        UpdateCarTransform();

        if (!isSwitching && IsAtEndOfSpline())
            OnJunctionEnter?.Invoke();

        if (isSwitching && !IsAtEndOfSpline())
            OnJunctionExit?.Invoke();

        if ((isForward && Dot < 0) || (!isForward && Dot > 0))
            isForward = !isForward;
    }

    #region Events
    private void Car_OnJunctionEnter()
    {
        isSwitching = true;
        Edge inputEdge = CurrentEdge;
        Edge nextEdge = null;

        Debug.Log($"Index Entered {Index}");

        // Only the leading car or caboose chooses the direction
        if ((Index == 0 && isForward) || (Index == Train.Size - 1 && !isForward))
        {
            bool shouldFlip = (!isForward && !isRotationBackwards) || (isForward && isRotationBackwards);
            inputEdge = shouldFlip ? GraphGenerator.GetInverse(CurrentEdge) : CurrentEdge;

            if (shouldFlip)
            {
                Debug.Log($"isRotationBackwards Flipped! {isRotationBackwards}");
                isRotationBackwards = !isRotationBackwards;
            }

            nextEdge = GraphGenerator.GetNextEdge(inputEdge);
        }
        // Other cars follow
        else
        {
            int adjacentCarIndex = isForward ? Index - 1 : Index + 1;
            nextEdge = Train.Cars[adjacentCarIndex].CurrentEdge;
            isRotationBackwards = Train.Cars[adjacentCarIndex].isRotationBackwards;
        }

        if (nextEdge == null)
        {
            OnDeadEnd?.Invoke();
            return;
        }

        OnEdgeChanged?.Invoke(nextEdge);
        UpdateCarTransform();
    }

    private void Car_OnJunctionExit()
    {
        isSwitching = false;
    }

    private void Car_OnEdgeChanged(Edge edge)
    {
        CurrentEdge = edge;
        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
    }

    private void Car_OnDeadEnd()
    {
        Rigidbody.velocity = Vector3.zero;
    }
    #endregion

    #region Car Transform
    private void UpdateCarTransform()
    {
        NativeSpline nativeSpline = new NativeSpline(CurrentSpline);

        SplineUtility.GetNearestPoint(nativeSpline, transform.localPosition, out float3 nearestPoint, out float newT);
        SplineUtility.GetNearestPoint(nativeSpline, transform.localPosition + transform.forward * AttachToCar.BoxCollider.size.z * 0.25f, out float3 forwardPoint, out forwardT);
        SplineUtility.GetNearestPoint(nativeSpline, transform.localPosition - transform.forward * AttachToCar.BoxCollider.size.z * 0.25f, out float3 backwardPoint, out backwardT);

        t = Mathf.Clamp01(newT);
        forwardT = Mathf.Clamp01(forwardT);
        backwardT = Mathf.Clamp01(backwardT);

        WantedPosition = (Vector3)nearestPoint;

        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));
        Vector3 up = nativeSpline.EvaluateUpVector(t);
        WantedRotation = isRotationBackwards ? Quaternion.LookRotation(-forward, up) : Quaternion.LookRotation(forward, up);

        transform.localPosition = Vector3.Lerp(Rigidbody.position, WantedPosition, positionLerpSpeed * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Slerp(transform.rotation, WantedRotation, rotationLerpSpeed * Time.fixedDeltaTime);

        Rigidbody.velocity = Rigidbody.velocity.magnitude * GetEngineForward();
    }

    protected Vector3 GetEngineForward()
    {
        return (Dot < 0) ? WantedRotation * Vector3.back : WantedRotation * Vector3.forward;
    }

    private bool IsAtEndOfSpline()
    {
        return isForward ? forwardT >= 1 : backwardT <= 0;
    }
    #endregion

    #region Gizmos
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
    #endregion
}