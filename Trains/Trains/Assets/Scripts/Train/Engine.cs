using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Engine : Car
{
    [Range(-10, 10)][SerializeField] private float power = 0;

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
        base.FixedUpdate();

        if (Input.GetKey(KeyCode.W))
        {
            Throttle(power);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Throttle(-power);
        }
    }
}