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
    public int walkableMask = LayerMask.GetMask("Walkable");
    public bool forceExit = false;

    public UnitCarry(NavMeshAgent navMeshAgent, Rigidbody rb, Transform tf, CarryData carryData)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
        _tf = tf;
        _carryData = carryData;
    }

    public void Tick()
    {
        // Change position to match the ground
        Vector3 targetPosition = _tf.position;
        Vector3 origin = _carryData.carryTarget.transform.position + _carryData.carryPivot + 0.5f * Vector3.up;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, 1f, walkableMask)){
            targetPosition = hit.point; // probably needs an offset
        } 
        _tf.position = Vector3.MoveTowards(_tf.position,
                                           targetPosition, 
                                           0.01f);

        Vector3 toRotate =  _carryData.carryTarget.transform.position + ((carryable.CurrCarryOffset + 0.5f) * Vector3.down) - _tf.position;
        _tf.rotation = Quaternion.LookRotation(Vector3.RotateTowards(_tf.forward, toRotate, 0.1f, 0.1f));
        // Assess if should continue carrying
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = true;
        _tf.parent = _carryData.carryTarget.transform;
        carryable = _carryData.carryTarget.GetComponent<ICarryable>();
        carryable.exitCallback += OnCarryExit;
        carryable.Carriers += 1;
        forceExit = false;
    }

    public void OnCarryExit(){
        forceExit = true;
    }

    public void OnExit(){
        _tf.parent = null;
        carryable.Carriers -= 1;
        carryable.exitCallback -= OnCarryExit;
        carryable = null;
        
    }

    public void OnCollisionEnter(Collision collision){}
}
