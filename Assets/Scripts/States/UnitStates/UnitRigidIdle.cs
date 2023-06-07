using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitRigidIdle : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;

    public UnitRigidIdle(NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
    }

    public void Tick(){
        Debug.Log(_rb.velocity);
    }
    public void OnExit(){}
    public void OnCollisionEnter(Collision collision){}
}
