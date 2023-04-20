using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class UnitFindNavMesh : IState
{
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;

    public UnitFindNavMesh(NavMeshAgent navMeshAgent, Rigidbody rb)
    {
        _navMeshAgent = navMeshAgent;
        _rb = rb;
    }

    public void Tick(){}

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
        _rb.isKinematic = true;
    }

    public void OnExit(){}
}
