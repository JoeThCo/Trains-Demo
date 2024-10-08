using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Engine))]
[RequireComponent(typeof(Car))]
public class Train : MonoBehaviour
{
    public List<Car> Cars { get; private set; }

    public delegate void CarChange();

    public event CarChange CarAdded;
    public event CarChange CarRemoved;

    private void Start()
    {
        Cars = GetConnectedCars();

        CarAdded += Train_CarAdded;
        CarRemoved += Train_CarRemoved;

        InjectTrain();
    }

    private void OnDisable()
    {
        CarAdded -= Train_CarAdded;
        CarRemoved -= Train_CarRemoved;
    }

    private void Train_CarRemoved()
    {
        throw new System.NotImplementedException();
    }

    private void Train_CarAdded()
    {
        throw new System.NotImplementedException();
    }

    void InjectTrain()
    {
        foreach (Car car in Cars)
            car.SetTrain(this);
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
                break;
        }
        return connectedCars;
    }
}