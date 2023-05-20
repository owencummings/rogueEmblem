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

    public UnitCarry(NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick()
    {
        // Assess if should continue carrying
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = true;
        Debug.Log("Carrying");
        // Turn toward thing?
    }

    public void OnExit(){}

    public void OnCollisionEnter(Collision collision){}
}
