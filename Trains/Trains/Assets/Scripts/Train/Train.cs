using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Train : MonoBehaviour
{
    public List<Car> Cars { get; private set; }
    public int Size { get { return Cars.Count; } }

    public delegate void CarChange(Car car);

    public event CarChange CarAdded;
    public event CarChange CarRemoved;

    private void Start()
    {
        Cars = new List<Car>();

        CarAdded += Train_CarAdded;
        CarRemoved += Train_CarRemoved;

        foreach (Transform t in transform)
        {
            Car car = t.GetComponent<Car>();
            if (car)
                CarAdded?.Invoke(car);
        }

        SetUpJoints();
        CarInfoInit();
    }

    private void OnDisable()
    {
        CarAdded -= Train_CarAdded;
        CarRemoved -= Train_CarRemoved;
    }

    #region Events
    private void Train_CarAdded(Car car)
    {
        if (car && !Cars.Contains(car))
        {
            Cars.Add(car);
            UpdateCarIndex();
        }
    }

    private void Train_CarRemoved(Car car)
    {
        if (car && Cars.Contains(car))
        {
            Cars.Remove(car);
            UpdateCarIndex();
        }
    }
    #endregion

    private void CarInfoInit()
    {
        for (int i = 0; i < Cars.Count; i++)
            Cars[i].SetTrain(this);
    }

    private void UpdateCarIndex()
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