using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Car))]
public class Engine : MonoBehaviour
{
    [Range(1, 10)][SerializeField] private float power = 0;

    public Car Car { get; private set; }
    public Rigidbody Rigidbody { get; private set; }

    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Car = GetComponent<Car>();
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Car.Throttle(power);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Car.Throttle(-power);
        }
    }
}