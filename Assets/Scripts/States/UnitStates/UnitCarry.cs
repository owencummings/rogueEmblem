using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarryData {
    public GameObject carryTarget;
    public Vector3 carryPivot;
}

public class UnitCarry : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Transform _tf;
    private CarryData _carryData;
    public ICarryable carryable;

    public UnitCarry(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf, CarryData carryData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        _carryData = carryData;
    }

    public void Tick()
    {
        // Move towards carry pivot 
        _tf.position = Vector3.MoveTowards(_tf.position,
                                           _carryData.carryTarget.transform.position + _carryData.carryPivot + carryable.CurrCarryOffset * Vector3.down, 
                                           0.01f);
        Vector3 toRotate =  _carryData.carryTarget.transform.position - _tf.position;
        toRotate = new Vector3(toRotate.x, 0, toRotate.z);
        _tf.rotation = Quaternion.LookRotation(Vector3.RotateTowards(_tf.forward, toRotate, 0.1f, 0.1f));
        // Assess if should continue carrying
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = true;
        _tf.parent = _carryData.carryTarget.transform;
        carryable = _carryData.carryTarget.GetComponent<ICarryable>();
        carryable.Carriers += 1;
    }

    public void OnExit(){
        _tf.parent = null;
        carryable.Carriers -= 1;
        carryable = null;
    }

    public void OnCollisionEnter(Collision collision){}
}
