using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitIdle : IState
{

    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;

    public UnitIdle(NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }


    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
    }

    public void Tick(){}
    public void OnExit(){}
    public void OnCollisionEnter(Collision collision){}
}

