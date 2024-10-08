using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Engine))]
[RequireComponent(typeof(Car))]
public class Train : MonoBehaviour
{
    public List<Car> Cars { get; set; }

    private void Start()
    {
        Cars = GetConnectedCars();
        InjectTrain();
    }

    void InjectTrain()
    {
        foreach (Car car in Cars)
        {
            car.SetTrain(this);
        }
    }

    List<Car> GetConnectedCars()
    {
        List<Car> connectedCars = new List<Car>();

        Car initialCar = GetComponent<Car>();
        connectedCars.Add(initialCar);

        ConfigurableJoint joint = GetComponent<ConfigurableJoint>();

        while (joint && joint.connectedBody)
        {
            Car car = joint.connectedBody.GetComponent<Car>();

            if (car)
            {
                connectedCars.Add(car);
                joint = car.GetComponent<ConfigurableJoint>();
            }
            else
            {
                break;
            }
        }

        return connectedCars;
    }
}