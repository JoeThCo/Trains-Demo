using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Train : MonoBehaviour
{
    public List<Car> Cars { get; private set; }

    public delegate void CarChange(Car car);

    public event CarChange CarAdded;
    public event CarChange CarRemoved;

    private void Start()
    {
        Cars = GetComponentsInChildren<Car>().ToList();

        foreach (Car car in Cars)
            CarAdded?.Invoke(car);

        CarAdded += Train_CarAdded;
        CarRemoved += Train_CarRemoved;

        SetUpJoints();
    }

    private void OnDisable()
    {
        CarAdded -= Train_CarAdded;
        CarRemoved -= Train_CarRemoved;
    }

    private void Train_CarRemoved(Car car)
    {
        if (car && Cars.Contains(car))
        {
            Cars.Remove(car);
            UpdateCarIndexes();
        }
    }

    private void Train_CarAdded(Car car)
    {
        if (car && !Cars.Contains(car))
        {
            Cars.Add(car);
            UpdateCarIndexes();
        }
    }

    private void UpdateCarIndexes()
    {
        for (int i = 0; i < Cars.Count; i++)
            Cars[i].SetTrainIndex(i);
    }

    private void SetUpJoints()
    {
        //Debug.Log(Cars.Count);
        for (int i = 1; i < Cars.Count; i++)
        {
            Car current = Cars[i - 1];
            Car behind = Cars[i];

            ConfigurableJoint behindJoint = behind.GetComponent<ConfigurableJoint>();
            behindJoint.connectedBody = current.Rigidbody;
        }
    }
}