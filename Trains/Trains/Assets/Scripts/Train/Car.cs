using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    public int Index { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Edge CurrentEdge { get; private set; }
    private Spline CurrentSpline { get; set; }

    public Vector3 WantedPosition { get; private set; }
    public Quaternion WantedRotation { get; private set; }

    [Header("Car")]
    [Range(0, 500)][SerializeField] private int positionLerpSpeed = 15;
    [Range(0, 500)][SerializeField] private int rotationLerpSpeed = 15;
    
    [Space(10)]
    
    [SerializeField] private float gizmoLineDistance = 7.5f;

    private float t = 0;
    public float Dot { get; private set; }

    private bool isSwitching = false;
    private bool isForward = true;
    private bool isRotationBackwards = false;

    private const float ENTER_EPSILON = .005f;

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

        Car_OnEdgeChanged(GraphGenerator.GetEdge(0));
        Debug.LogWarning($"Edge: {CurrentEdge.Index}");

        transform.rotation = Quaternion.Euler(CurrentEdge.EdgeDireciton);
        FixedUpdate();

        Rigidbody.velocity = Vector3.zero;
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
        {
            //Debug.Log($"isForwards Flipped! {isForward}");
            isForward = !isForward;
        }
    }

    #region Events
    private void Car_OnDeadEnd()
    {
        Rigidbody.velocity = Vector3.zero;
    }

    private void Car_OnJunctionExit()
    {
        isSwitching = false;
    }

    private void Car_OnJunctionEnter()
    {
        isSwitching = true;
        Edge inputEdge = CurrentEdge;

        if (!isForward && !isRotationBackwards || isForward && isRotationBackwards)
        {
            Debug.Log($"isRotationBackwards Flipped! {isRotationBackwards}");
            isRotationBackwards = !isRotationBackwards;
            inputEdge = GraphGenerator.GetInverse(CurrentEdge);
        }

        Debug.LogWarning($"In: {inputEdge}");
        Edge nextEdge = GraphGenerator.GetNextEdge(inputEdge);
        if (nextEdge == null)
        {
            OnDeadEnd?.Invoke();
            return;
        }
        Debug.LogWarning($"In: {nextEdge}");
        OnEdgeChanged?.Invoke(nextEdge);
        UpdateCarTransform();
    }

    private void Car_OnEdgeChanged(Edge edge)
    {
        CurrentEdge = edge;
        CurrentSpline = GraphGenerator.GetSpline(CurrentEdge);
    }
    #endregion

    #region Car Transform
    private void UpdateCarTransform()
    {
        NativeSpline nativeSpline = new NativeSpline(CurrentSpline);
        SplineUtility.GetNearestPoint(nativeSpline, Rigidbody.position, out float3 nearestPoint, out float newT);
        t = Mathf.Clamp01(newT);
        WantedPosition = (Vector3)nearestPoint;

        Vector3 forward = Vector3.Normalize(CurrentSpline.EvaluateTangent(t));
        Vector3 up = nativeSpline.EvaluateUpVector(t);
        WantedRotation = isRotationBackwards ? Quaternion.LookRotation(-forward, up) : Quaternion.LookRotation(forward, up);

        Rigidbody.MovePosition(Vector3.Lerp(Rigidbody.position, WantedPosition, positionLerpSpeed * Time.fixedDeltaTime));
        Rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, WantedRotation, rotationLerpSpeed * Time.fixedDeltaTime));

        Rigidbody.velocity = Rigidbody.velocity.magnitude * GetEngineForward();
    }

    protected Vector3 GetEngineForward() 
    {
        Vector3 engineForward = WantedRotation * Vector3.forward;
        if (Dot < 0)
            engineForward *= -1;

        return engineForward;
    }

    private bool IsAtEndOfSpline()
    {
        return t <= 0 + ENTER_EPSILON || t >= 1 - ENTER_EPSILON;
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