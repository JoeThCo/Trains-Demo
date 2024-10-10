using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Engine : Car
{
    [Range(-10, 10)][SerializeField] private int accleration = 0;
    [Range(0, 100)][SerializeField] private int maxSpeed = 25;

    protected override void CarInit()
    {
        base.CarInit();
    }

    protected void Throttle(float power)
    {
        Rigidbody.AddForce(transform.forward * power);
    }

    protected override void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Throttle(accleration);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Throttle(-accleration);
        }

        base.FixedUpdate();

        if (Rigidbody.velocity.magnitude > maxSpeed)
        {
            Rigidbody.velocity = Rigidbody.velocity.normalized * maxSpeed;
        }
    }
}