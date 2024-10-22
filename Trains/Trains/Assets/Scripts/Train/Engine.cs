using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Engine : Car
{
    [Header("Engine")]
    [Range(1, 25)][SerializeField] private int accleration = 10;
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
        if (AttachToCar.isPlayerControllerAttached()) 
        {
            if (Input.GetKey(KeyCode.Q))
                Throttle(accleration);

            if (Input.GetKey(KeyCode.E))
                Throttle(-accleration);
        }

        base.FixedUpdate();

        if (Rigidbody.velocity.magnitude > maxSpeed)
            Rigidbody.velocity = Rigidbody.velocity.normalized * maxSpeed;
    }

    public string GetSpeedText()
    {
        return $"Speed: {(int)Rigidbody.velocity.magnitude} mph\nMax {(int)maxSpeed} mph";
    }
}